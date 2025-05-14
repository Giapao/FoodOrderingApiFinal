import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

interface Restaurant {
  id: number;
  name: string;
  description: string;
  address: string;
  phoneNumber: string;
}

const RestaurantSelector: React.FC = () => {
  const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const fetchRestaurants = async (searchTerm = '') => {
    setLoading(true);
    try {
      let url = '/api/user/restaurants';
      if (searchTerm) {
        url = `/api/user/restaurants/search?searchTerm=${encodeURIComponent(searchTerm)}`;
      }
      const token = localStorage.getItem('token');
      const response = await fetch(url, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });
      if (!response.ok) throw new Error('Failed to fetch restaurants');
      const data = await response.json();
      // Nếu là paged result
      const items = data.items || data;
      setRestaurants(items);
    } catch (err) {
      setError('Không thể tải danh sách nhà hàng');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRestaurants();
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchRestaurants(search);
  };

  return (
    <div className="max-w-3xl mx-auto py-8">
      <h2 className="text-2xl font-bold mb-4 text-center">Chọn nhà hàng để đặt món</h2>
      <form onSubmit={handleSearch} className="flex gap-2 mb-6 justify-center">
        <input
          type="text"
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Tìm kiếm nhà hàng..."
          className="border rounded px-3 py-2 w-64"
        />
        <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition-colors">Tìm kiếm</button>
      </form>
      {loading ? (
        <div className="text-center">Đang tải...</div>
      ) : error ? (
        <div className="text-center text-red-500">{error}</div>
      ) : restaurants.length === 0 ? (
        <div className="text-center">Không có nhà hàng nào.</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {restaurants.map(r => (
            <div key={r.id} className="border rounded-lg p-4 shadow hover:shadow-lg transition cursor-pointer bg-white" onClick={() => navigate(`/user/restaurants/${r.id}/menu`)}>
              <h3 className="text-xl font-semibold mb-2">{r.name}</h3>
              <p className="text-gray-600 mb-1">{r.description}</p>
              <p className="text-gray-500 text-sm">Địa chỉ: {r.address}</p>
              <p className="text-gray-500 text-sm">SĐT: {r.phoneNumber}</p>
              <button className="mt-3 bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 transition-colors">Xem menu</button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default RestaurantSelector; 