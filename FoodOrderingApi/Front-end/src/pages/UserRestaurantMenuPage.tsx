import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useCart } from '../contexts/CartContext';

interface MenuItem {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  isAvailable: boolean;
}

const UserRestaurantMenuPage: React.FC = () => {
  const { restaurantId } = useParams<{ restaurantId: string }>();
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const navigate = useNavigate();
  const { addToCart, loading: cartLoading } = useCart();

  useEffect(() => {
    const fetchMenu = async () => {
      setLoading(true);
      try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/user/restaurants/${restaurantId}/menu`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        });
        if (!response.ok) throw new Error('Không thể tải menu');
        const data = await response.json();
        // Nếu là paged result
        const items = data.items || data;
        setMenuItems(items);
      } catch (err) {
        setError('Không thể tải menu');
      } finally {
        setLoading(false);
      }
    };
    fetchMenu();
  }, [restaurantId]);

  const handleAddToCart = async (menuItemId: number) => {
    if (!restaurantId) return;
    try {
      await addToCart(Number(restaurantId), menuItemId, 1);
      setMessage('Đã thêm vào giỏ hàng!');
      setTimeout(() => setMessage(null), 2000);
    } catch (e) {
      setMessage('Không thể thêm vào giỏ hàng');
      setTimeout(() => setMessage(null), 2000);
    }
  };

  return (
    <div className="max-w-4xl mx-auto py-8">
      <button onClick={() => navigate(-1)} className="mb-4 text-blue-600 hover:underline">← Quay lại chọn nhà hàng</button>
      <h2 className="text-2xl font-bold mb-6 text-center">Menu nhà hàng</h2>
      {message && <div className="text-center mb-4 text-green-600 font-semibold">{message}</div>}
      {loading || cartLoading ? (
        <div className="text-center">Đang tải menu...</div>
      ) : error ? (
        <div className="text-center text-red-500">{error}</div>
      ) : menuItems.length === 0 ? (
        <div className="text-center">Nhà hàng chưa có món ăn nào.</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {menuItems.filter(item => item.isAvailable !== false).map(item => (
            <div key={item.id} className="border rounded-lg p-4 shadow bg-white">
              <img src={item.imageUrl} alt={item.name} className="h-32 w-full object-cover rounded mb-3" />
              <h3 className="text-lg font-semibold mb-1">{item.name}</h3>
              <p className="text-gray-600 mb-1">{item.description}</p>
              <p className="text-gray-900 font-bold mb-2">{item.price.toLocaleString('vi-VN')}đ</p>
              <button onClick={() => handleAddToCart(item.id)} className="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700 transition-colors">Đặt món</button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default UserRestaurantMenuPage; 