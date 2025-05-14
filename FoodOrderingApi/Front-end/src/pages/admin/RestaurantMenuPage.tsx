import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import AdminPageWrapper from '../../components/admin/AdminPageWrapper';

interface MenuItem {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  isAvailable: boolean;
  restaurantId?: number;
}

const RestaurantMenuPage: React.FC = () => {
  const { restaurantId } = useParams<{ restaurantId: string }>();
  const navigate = useNavigate();
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [editData, setEditData] = useState<Partial<MenuItem>>({});
  const [newMenuItem, setNewMenuItem] = useState<Omit<MenuItem, 'id'>>({
    name: '',
    description: '',
    price: 0,
    imageUrl: '',
    isAvailable: true,
    restaurantId: parseInt(restaurantId || '0')
  });

  const fetchMenuItems = async () => {
    try {
      const token = localStorage.getItem('token');
      console.log('Fetching menu items for restaurant:', restaurantId);
      const response = await fetch(`/api/restaurant/${restaurantId}/menuitem`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      if (!response.ok) {
        console.error('Response not OK:', response.status, response.statusText);
        throw new Error('Failed to fetch menu items');
      }
      const data = await response.json();
      console.log('Received menu items:', data);
      
      // Handle PagedResult response
      const items = data.items || [];
      const formattedData = items.map(item => ({
        ...item,
        isAvailable: item.isAvailable ?? true // Default to true if not specified
      }));
      console.log('Formatted menu items:', formattedData);
      setMenuItems(formattedData);
    } catch (error) {
      setError('Error loading menu items');
      console.error('Error:', error);
      setMenuItems([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMenuItems();
  }, [restaurantId]);

  const handleDelete = async (itemId: number) => {
    if (!window.confirm('Are you sure you want to delete this menu item?')) return;

    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`/api/restaurant/${restaurantId}/menuitem/${itemId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) throw new Error('Failed to delete menu item');
      
      setMenuItems(menuItems.filter(item => item.id !== itemId));
      setSuccessMessage('Menu item deleted successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error) {
      setError('Error deleting menu item');
      console.error('Error:', error);
    }
  };

  const handleToggleAvailability = async (itemId: number, currentStatus: boolean) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`/api/restaurant/${restaurantId}/menuitem/${itemId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ isAvailable: !currentStatus }),
      });

      if (!response.ok) throw new Error('Failed to update menu item status');
      
      setMenuItems(menuItems.map(item => 
        item.id === itemId 
          ? { ...item, isAvailable: !currentStatus }
          : item
      ));
      setSuccessMessage(`Menu item ${!currentStatus ? 'activated' : 'deactivated'} successfully`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error) {
      setError('Error updating menu item status');
      console.error('Error:', error);
    }
  };

  const handleEdit = (item: MenuItem) => {
    setEditId(item.id);
    setEditData({ ...item });
  };

  const handleEditChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setEditData((prev) => ({ ...prev, [name]: value }));
  };

  const handleEditSave = async (itemId: number) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`/api/restaurant/${restaurantId}/menuitem/${itemId}`, {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(editData),
      });
      if (!response.ok) throw new Error('Failed to update menu item');
      const updated = await response.json();
      setMenuItems(menuItems.map(item => item.id === itemId ? updated : item));
      setSuccessMessage('Menu item updated successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
      setEditId(null);
      setEditData({});
    } catch (error) {
      setError('Error updating menu item');
      console.error('Error:', error);
    }
  };

  const handleEditCancel = () => {
    setEditId(null);
    setEditData({});
  };

  const handleNewChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setNewMenuItem((prev) => ({ ...prev, [name]: value }));
  };

  const handleAddNew = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      const menuItemToAdd = {
        ...newMenuItem,
        restaurantId: parseInt(restaurantId || '0')
      };
      console.log('Adding new menu item:', menuItemToAdd);
      
      const response = await fetch(`/api/restaurant/${restaurantId}/menuitem`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(menuItemToAdd),
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        console.error('Failed to add menu item:', errorData);
        throw new Error('Failed to add menu item');
      }
      
      const created = await response.json();
      console.log('Created menu item:', created);
      setMenuItems([created, ...menuItems]);
      setSuccessMessage('Menu item added successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
      setNewMenuItem({ 
        name: '', 
        description: '', 
        price: 0, 
        imageUrl: '', 
        isAvailable: true,
        restaurantId: parseInt(restaurantId || '0')
      });
    } catch (error) {
      setError('Error adding menu item');
      console.error('Error:', error);
    }
  };

  return (
    <AdminPageWrapper loading={loading} error={error}>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">Menu Items Management</h1>
          <button
            onClick={() => navigate('/admin/restaurants')}
            className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600 transition-colors"
          >
            Back to Restaurants
          </button>
        </div>

        {/* Add new menu item form */}
        <form onSubmit={handleAddNew} className="bg-white rounded-lg shadow p-4 mb-4 flex flex-wrap gap-4 items-end">
          <input
            name="name"
            value={newMenuItem.name}
            onChange={handleNewChange}
            placeholder="Name"
            className="border rounded px-2 py-1 flex-1 min-w-[120px]"
            required
          />
          <input
            name="price"
            type="number"
            value={newMenuItem.price}
            onChange={handleNewChange}
            placeholder="Price"
            className="border rounded px-2 py-1 flex-1 min-w-[120px]"
            required
          />
          <input
            name="imageUrl"
            value={newMenuItem.imageUrl}
            onChange={handleNewChange}
            placeholder="Image URL"
            className="border rounded px-2 py-1 flex-1 min-w-[120px]"
            required
          />
          <textarea
            name="description"
            value={newMenuItem.description}
            onChange={handleNewChange}
            placeholder="Description"
            className="border rounded px-2 py-1 flex-1 min-w-[120px]"
            required
          />
          <button
            type="submit"
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition-colors"
          >
            Add
          </button>
        </form>

        {successMessage && (
          <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded">
            {successMessage}
          </div>
        )}

        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Price</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Image</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {menuItems.map((item) => (
                <tr key={item.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">{item.id}</td>
                  <td className="px-6 py-4 whitespace-nowrap font-medium">
                    {editId === item.id ? (
                      <input
                        name="name"
                        value={editData.name || ''}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      item.name
                    )}
                  </td>
                  <td className="px-6 py-4 max-w-xs truncate">
                    {editId === item.id ? (
                      <textarea
                        name="description"
                        value={editData.description || ''}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      item.description
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {editId === item.id ? (
                      <input
                        name="price"
                        type="number"
                        value={editData.price || 0}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      `$${item.price.toFixed(2)}`
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {editId === item.id ? (
                      <input
                        name="imageUrl"
                        value={editData.imageUrl || ''}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      <img src={item.imageUrl} alt={item.name} className="h-10 w-10 object-cover rounded" />
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <button
                      onClick={() => handleToggleAvailability(item.id, item.isAvailable)}
                      className={`px-3 py-1 rounded-full text-sm font-semibold transition-colors ${
                        item.isAvailable 
                          ? 'bg-green-100 text-green-800 hover:bg-green-200' 
                          : 'bg-red-100 text-red-800 hover:bg-red-200'
                      }`}
                    >
                      {item.isAvailable ? 'Available' : 'Unavailable'}
                    </button>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap space-x-2">
                    {editId === item.id ? (
                      <>
                        <button
                          onClick={() => handleEditSave(item.id)}
                          className="text-green-600 hover:text-green-900 transition-colors"
                        >
                          Save
                        </button>
                        <button
                          onClick={handleEditCancel}
                          className="text-gray-600 hover:text-gray-900 transition-colors"
                        >
                          Cancel
                        </button>
                      </>
                    ) : (
                      <>
                        <button
                          onClick={() => handleEdit(item)}
                          className="text-blue-600 hover:text-blue-900 transition-colors"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(item.id)}
                          className="text-red-600 hover:text-red-900 transition-colors"
                        >
                          Delete
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </AdminPageWrapper>
  );
};

export default RestaurantMenuPage; 