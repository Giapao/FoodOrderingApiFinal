import React, { useEffect, useState } from 'react';
import AdminPageWrapper from '../../components/admin/AdminPageWrapper';

interface Restaurant {
  id: number;
  name: string;
  description: string;
  address: string;
  phoneNumber: string;
  isActive: boolean;
}

const RestaurantsPage: React.FC = () => {
  const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [editId, setEditId] = useState<number | null>(null);
  const [editData, setEditData] = useState<Partial<Restaurant>>({});
  const [newRestaurant, setNewRestaurant] = useState<Omit<Restaurant, 'id' | 'isActive'>>({
    name: '',
    description: '',
    address: '',
    phoneNumber: '',
  });

  const fetchRestaurants = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('/api/Admin/restaurants', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      if (!response.ok) throw new Error('Failed to fetch restaurants');
      const data = await response.json();
      setRestaurants(data);
    } catch (error) {
      setError('Error loading restaurants');
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRestaurants();
  }, []);

  const handleDelete = async (restaurantId: number) => {
    if (!window.confirm('Are you sure you want to delete this restaurant?')) return;

    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`/api/Admin/restaurants/${restaurantId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) throw new Error('Failed to delete restaurant');
      
      setRestaurants(restaurants.filter(restaurant => restaurant.id !== restaurantId));
      setSuccessMessage('Restaurant deleted successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error) {
      setError('Error deleting restaurant');
      console.error('Error:', error);
    }
  };

  const handleToggleStatus = async (restaurantId: number, currentStatus: boolean) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`/api/Admin/restaurants/${restaurantId}/status`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ isActive: !currentStatus }),
      });

      if (!response.ok) throw new Error('Failed to update restaurant status');
      
      setRestaurants(restaurants.map(restaurant => 
        restaurant.id === restaurantId 
          ? { ...restaurant, isActive: !currentStatus }
          : restaurant
      ));
      setSuccessMessage(`Restaurant ${!currentStatus ? 'activated' : 'deactivated'} successfully`);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error) {
      setError('Error updating restaurant status');
      console.error('Error:', error);
    }
  };

  const handleEdit = (restaurant: Restaurant) => {
    setEditId(restaurant.id);
    setEditData({ ...restaurant });
  };

  const handleEditChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setEditData((prev) => ({ ...prev, [name]: value }));
  };

  const handleEditSave = async (restaurantId: number) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`/api/Admin/restaurants/${restaurantId}`, {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(editData),
      });
      if (!response.ok) throw new Error('Failed to update restaurant');
      const updated = await response.json();
      setRestaurants(restaurants.map(r => r.id === restaurantId ? updated : r));
      setSuccessMessage('Restaurant updated successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
      setEditId(null);
      setEditData({});
    } catch (error) {
      setError('Error updating restaurant');
      console.error('Error:', error);
    }
  };

  const handleEditCancel = () => {
    setEditId(null);
    setEditData({});
  };

  const handleNewChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setNewRestaurant((prev) => ({ ...prev, [name]: value }));
  };

  const handleAddNew = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('/api/Admin/restaurants', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(newRestaurant),
      });
      if (!response.ok) throw new Error('Failed to add restaurant');
      const created = await response.json();
      setRestaurants([created, ...restaurants]);
      setSuccessMessage('Restaurant added successfully');
      setTimeout(() => setSuccessMessage(null), 3000);
      setNewRestaurant({ name: '', description: '', address: '', phoneNumber: '' });
    } catch (error) {
      setError('Error adding restaurant');
      console.error('Error:', error);
    }
  };

  return (
    <AdminPageWrapper loading={loading} error={error}>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">Restaurants Management</h1>
        </div>

        {/* Add new restaurant form */}
        <form onSubmit={handleAddNew} className="bg-white rounded-lg shadow p-4 mb-4 flex flex-wrap gap-4 items-end">
          <input
            name="name"
            value={newRestaurant.name}
            onChange={handleNewChange}
            placeholder="Name"
            className="border rounded px-2 py-1 flex-1 min-w-[120px]"
            required
          />
          <input
            name="address"
            value={newRestaurant.address}
            onChange={handleNewChange}
            placeholder="Address"
            className="border rounded px-2 py-1 flex-1 min-w-[120px]"
            required
          />
          <input
            name="phoneNumber"
            value={newRestaurant.phoneNumber}
            onChange={handleNewChange}
            placeholder="Phone"
            className="border rounded px-2 py-1 flex-1 min-w-[120px]"
            required
          />
          <textarea
            name="description"
            value={newRestaurant.description}
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
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Address</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Phone</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {restaurants.map((restaurant) => (
                <tr key={restaurant.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">{restaurant.id}</td>
                  <td className="px-6 py-4 whitespace-nowrap font-medium">
                    {editId === restaurant.id ? (
                      <input
                        name="name"
                        value={editData.name || ''}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      restaurant.name
                    )}
                  </td>
                  <td className="px-6 py-4 max-w-xs truncate">
                    {editId === restaurant.id ? (
                      <textarea
                        name="description"
                        value={editData.description || ''}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      restaurant.description
                    )}
                  </td>
                  <td className="px-6 py-4">
                    {editId === restaurant.id ? (
                      <input
                        name="address"
                        value={editData.address || ''}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      restaurant.address
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {editId === restaurant.id ? (
                      <input
                        name="phoneNumber"
                        value={editData.phoneNumber || ''}
                        onChange={handleEditChange}
                        className="border rounded px-2 py-1 w-full"
                      />
                    ) : (
                      restaurant.phoneNumber
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <button
                      onClick={() => handleToggleStatus(restaurant.id, restaurant.isActive)}
                      className={`px-3 py-1 rounded-full text-sm font-semibold transition-colors ${
                        restaurant.isActive 
                          ? 'bg-green-100 text-green-800 hover:bg-green-200' 
                          : 'bg-red-100 text-red-800 hover:bg-red-200'
                      }`}
                    >
                      {restaurant.isActive ? 'Active' : 'Inactive'}
                    </button>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap space-x-2">
                    {editId === restaurant.id ? (
                      <>
                        <button
                          onClick={() => handleEditSave(restaurant.id)}
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
                          onClick={() => handleEdit(restaurant)}
                          className="text-blue-600 hover:text-blue-900 transition-colors"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(restaurant.id)}
                          className="text-red-600 hover:text-red-900 transition-colors"
                        >
                          Delete
                        </button>
                        <a
                          href={`/admin/restaurants/${restaurant.id}/menu`}
                          className="text-purple-600 hover:text-purple-900 transition-colors"
                        >
                          Manage Menu
                        </a>
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

export default RestaurantsPage; 