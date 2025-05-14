import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import RestaurantSelector from './RestaurantSelector';

const HomePage: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return (
    <div className="bg-gray-50">
      {/* Hero Section */}
      <div className="relative bg-indigo-800">
        <div className="absolute inset-0">
          <img
            className="w-full h-full object-cover"
            src="https://images.unsplash.com/photo-1504674900247-0877df9cc836?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80"
            alt="Food background"
          />
          <div className="absolute inset-0 bg-indigo-800 mix-blend-multiply" />
        </div>
        <div className="relative max-w-7xl mx-auto py-24 px-4 sm:py-32 sm:px-6 lg:px-8">
          <h1 className="text-4xl font-extrabold tracking-tight text-white sm:text-5xl lg:text-6xl">
            Chào mừng đến với Food Ordering
          </h1>
          <p className="mt-6 text-xl text-indigo-100 max-w-3xl">
            Khám phá thực đơn phong phú của chúng tôi và đặt món ngay hôm nay!
          </p>
        </div>
      </div>

      {/* Call to Action */}
      {!isAuthenticated && (
        <div className="bg-indigo-700">
          <div className="max-w-2xl mx-auto text-center py-16 px-4 sm:py-20 sm:px-6 lg:px-8">
            <h2 className="text-3xl font-extrabold text-white sm:text-4xl">
              <span className="block">Bắt đầu đặt món ngay hôm nay</span>
            </h2>
            <p className="mt-4 text-lg leading-6 text-indigo-200">
              Đăng ký tài khoản để nhận nhiều ưu đãi và dễ dàng theo dõi đơn hàng của bạn.
            </p>
            <Link
              to="/register"
              className="mt-8 w-full inline-flex items-center justify-center px-5 py-3 border border-transparent text-base font-medium rounded-md text-indigo-600 bg-white hover:bg-indigo-50 sm:w-auto"
            >
              Đăng ký ngay
            </Link>
          </div>
        </div>
      )}

      <div className="bg-gray-50 min-h-[400px] py-8">
        <RestaurantSelector />
      </div>
    </div>
  );
};

export default HomePage; 