import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';

interface Order {
  id: number;
  orderDate: string;
  status: string;
  totalAmount: number;
  deliveryAddress: string;
  phoneNumber: string;
  specialInstructions?: string;
  restaurantName: string;
  orderDetails: OrderDetail[];
}

interface OrderDetail {
  productId: number;
  quantity: number;
  unitPrice: number;
  productName: string;
}

interface ChangePasswordForm {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

const UserProfilePage: React.FC = () => {
  const { user } = useAuth();
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [passwordForm, setPasswordForm] = useState<ChangePasswordForm>({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
  });
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null);
  const [isChangingPassword, setIsChangingPassword] = useState(false);

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('http://localhost:5182/api/Orders/user', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to fetch orders');
      }

      const data = await response.json();
      setOrders(data);
    } catch (err) {
      setError('Error loading orders');
      console.error('Error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setPasswordForm(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordError(null);
    setPasswordSuccess(null);

    // Validate passwords
    if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
      setPasswordError('Mật khẩu mới không khớp');
      return;
    }

    if (passwordForm.newPassword.length < 8) {
      setPasswordError('Mật khẩu mới phải có ít nhất 8 ký tự');
      return;
    }

    // Validate password complexity
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/;
    if (!passwordRegex.test(passwordForm.newPassword)) {
      setPasswordError('Mật khẩu mới phải chứa ít nhất một chữ hoa, một chữ thường, một số và một ký tự đặc biệt');
      return;
    }

    try {
      setIsChangingPassword(true);
      const token = localStorage.getItem('token');
      const response = await fetch('http://localhost:5182/api/Auth/change-password', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          CurrentPassword: passwordForm.currentPassword,
          NewPassword: passwordForm.newPassword,
          ConfirmNewPassword: passwordForm.confirmNewPassword
        })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Không thể đổi mật khẩu');
      }

      setPasswordSuccess('Đổi mật khẩu thành công');
      setPasswordForm({
        currentPassword: '',
        newPassword: '',
        confirmNewPassword: ''
      });
    } catch (err) {
      setPasswordError(err instanceof Error ? err.message : 'Có lỗi xảy ra khi đổi mật khẩu');
    } finally {
      setIsChangingPassword(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'confirmed':
        return 'bg-blue-100 text-blue-800';
      case 'prepared':
        return 'bg-purple-100 text-purple-800';
      case 'completed':
        return 'bg-green-100 text-green-800';
      case 'cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return <div className="flex justify-center items-center min-h-screen">Loading...</div>;
  }

  if (error) {
    return <div className="text-red-500 text-center mt-4">{error}</div>;
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="bg-white shadow rounded-lg p-6 mb-8">
        <h2 className="text-2xl font-bold mb-4">Thông tin tài khoản</h2>
        <div className="grid grid-cols-2 gap-4 mb-6">
          <div>
            <p className="text-gray-600">Email:</p>
            <p className="font-semibold">{user?.email}</p>
          </div>
          <div>
            <p className="text-gray-600">Vai trò:</p>
            <p className="font-semibold">{user?.role}</p>
          </div>
        </div>

        {/* Change Password Form */}
        <div className="mt-6">
          <h3 className="text-xl font-semibold mb-4">Đổi mật khẩu</h3>
          <form onSubmit={handlePasswordSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">Mật khẩu hiện tại</label>
              <input
                type="password"
                name="currentPassword"
                value={passwordForm.currentPassword}
                onChange={handlePasswordChange}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Mật khẩu mới</label>
              <input
                type="password"
                name="newPassword"
                value={passwordForm.newPassword}
                onChange={handlePasswordChange}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Xác nhận mật khẩu mới</label>
              <input
                type="password"
                name="confirmNewPassword"
                value={passwordForm.confirmNewPassword}
                onChange={handlePasswordChange}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                required
              />
            </div>
            {passwordError && (
              <div className="text-red-500 text-sm">{passwordError}</div>
            )}
            {passwordSuccess && (
              <div className="text-green-500 text-sm">{passwordSuccess}</div>
            )}
            <button
              type="submit"
              disabled={isChangingPassword}
              className={`w-full bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700 ${
                isChangingPassword ? 'opacity-50 cursor-not-allowed' : ''
              }`}
            >
              {isChangingPassword ? 'Đang xử lý...' : 'Đổi mật khẩu'}
            </button>
          </form>
        </div>
      </div>

      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-2xl font-bold mb-4">Lịch sử đơn hàng</h2>
        {orders.length === 0 ? (
          <p className="text-gray-500 text-center">Chưa có đơn hàng nào</p>
        ) : (
          <div className="space-y-6">
            {orders.map((order) => (
              <div key={order.id} className="border rounded-lg p-4">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-lg font-semibold">Đơn hàng #{order.id}</h3>
                    <p className="text-gray-600">{order.restaurantName}</p>
                    <p className="text-gray-600">Ngày đặt: {formatDate(order.orderDate)}</p>
                  </div>
                  <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(order.status)}`}>
                    {order.status}
                  </span>
                </div>

                <div className="mb-4">
                  <h4 className="font-medium mb-2">Thông tin giao hàng:</h4>
                  <p className="text-gray-600">Địa chỉ: {order.deliveryAddress}</p>
                  <p className="text-gray-600">Số điện thoại: {order.phoneNumber}</p>
                  {order.specialInstructions && (
                    <p className="text-gray-600">Ghi chú: {order.specialInstructions}</p>
                  )}
                </div>

                <div className="mb-4">
                  <h4 className="font-medium mb-2">Chi tiết đơn hàng:</h4>
                  <div className="space-y-2">
                    {order.orderDetails.map((detail, index) => (
                      <div key={index} className="flex text-gray-600">
                        <span className="font-medium">{detail.productName}</span>
                        <span className="ml-2">x {detail.quantity}</span>
                      </div>
                    ))}
                  </div>
                </div>

                <div className="flex justify-between items-center pt-4 border-t">
                  <span className="font-medium">Tổng tiền:</span>
                  <span className="font-bold text-lg">{order.totalAmount.toLocaleString('vi-VN')}đ</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default UserProfilePage; 