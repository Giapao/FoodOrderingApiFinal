import React, { useState } from 'react';
import { useCart } from '../contexts/CartContext';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface CartDrawerProps {
  open: boolean;
  onClose: () => void;
  restaurantId?: number;
}

interface DeliveryInfo {
  deliveryAddress: string;
  phoneNumber: string;
  specialInstructions: string;
}

const CartDrawer: React.FC<CartDrawerProps> = ({ open, onClose, restaurantId }) => {
  const { cart, removeFromCart, loading, createOrder } = useCart();
  const { isAdmin } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const [isCheckingOut, setIsCheckingOut] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [deliveryInfo, setDeliveryInfo] = useState<DeliveryInfo>({
    deliveryAddress: '',
    phoneNumber: '',
    specialInstructions: ''
  });
  const navigate = useNavigate();
  
  const filteredCart = restaurantId ? cart.filter(i => i.restaurantId === restaurantId) : cart;
  const total = filteredCart.reduce((sum, item) => sum + item.price * item.quantity, 0);

  const handleCheckout = async () => {
    if (!deliveryInfo.deliveryAddress || !deliveryInfo.phoneNumber) {
      setError('Vui lòng nhập đầy đủ thông tin giao hàng');
      return;
    }
    try {
      setError(null);
      setIsCheckingOut(true);
      const orderData = await createOrder(deliveryInfo);
      setSuccessMessage('Đặt hàng thành công!');
      onClose();
      
      // Chuyển hướng sau 2 giây
      setTimeout(() => {
        if (isAdmin) {
          navigate('/admin/orders');
        } else {
          navigate('/profile');
        }
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Có lỗi xảy ra khi đặt hàng');
    } finally {
      setIsCheckingOut(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setDeliveryInfo(prev => ({
      ...prev,
      [name]: value
    }));
  };

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-end">
      <div className="fixed inset-0 bg-black opacity-30" onClick={onClose}></div>
      <div className="relative w-full max-w-md bg-white h-full shadow-lg p-6 overflow-y-auto">
        <button className="absolute top-2 right-2 text-gray-500 hover:text-black" onClick={onClose}>✕</button>
        <h2 className="text-xl font-bold mb-4">Giỏ hàng</h2>
        {loading ? (
          <div>Đang tải...</div>
        ) : filteredCart.length === 0 ? (
          <div>Giỏ hàng trống.</div>
        ) : (
          <>
            <ul className="divide-y mb-4">
              {filteredCart.map(item => (
                <li key={item.menuItemId} className="py-2 flex items-center gap-3">
                  <img src={item.imageUrl} alt={item.name} className="w-12 h-12 object-cover rounded" />
                  <div className="flex-1">
                    <div className="font-semibold">{item.name}</div>
                    <div className="text-sm text-gray-500">SL: {item.quantity}</div>
                    <div className="text-sm text-gray-700">{item.price.toLocaleString('vi-VN')}đ</div>
                  </div>
                  <button 
                    onClick={() => removeFromCart(item.restaurantId, item.menuItemId)}
                    className="text-red-500 hover:underline text-sm"
                    disabled={isCheckingOut}
                  >
                    Xóa
                  </button>
                </li>
              ))}
            </ul>
            <div className="font-bold mb-4">Tổng: {total.toLocaleString('vi-VN')}đ</div>

            {/* Delivery Information Form */}
            <div className="mb-4">
              <h3 className="font-semibold mb-2">Thông tin giao hàng</h3>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Địa chỉ giao hàng *</label>
                  <input
                    type="text"
                    name="deliveryAddress"
                    value={deliveryInfo.deliveryAddress}
                    onChange={handleInputChange}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                    placeholder="Nhập địa chỉ giao hàng"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Số điện thoại *</label>
                  <input
                    type="tel"
                    name="phoneNumber"
                    value={deliveryInfo.phoneNumber}
                    onChange={handleInputChange}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                    placeholder="Nhập số điện thoại"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Ghi chú</label>
                  <textarea
                    name="specialInstructions"
                    value={deliveryInfo.specialInstructions}
                    onChange={handleInputChange}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                    placeholder="Nhập ghi chú (nếu có)"
                    rows={2}
                  />
                </div>
              </div>
            </div>

            {error && (
              <div className="text-red-500 mb-4">{error}</div>
            )}
            {successMessage && (
              <div className="text-green-500 mb-4">{successMessage}</div>
            )}
            <button 
              onClick={handleCheckout}
              disabled={isCheckingOut}
              className={`w-full bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700 ${
                isCheckingOut ? 'opacity-50 cursor-not-allowed' : ''
              }`}
            >
              {isCheckingOut ? 'Đang xử lý...' : 'Đặt hàng'}
            </button>
          </>
        )}
      </div>
    </div>
  );
};

export default CartDrawer; 