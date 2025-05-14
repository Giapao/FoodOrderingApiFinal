import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface CartItem {
  id: number;
  name: string;
  price: number;
  quantity: number;
  imageUrl: string;
  restaurantId: number;
  menuItemId: number;
}

interface DeliveryInfo {
  deliveryAddress: string;
  phoneNumber: string;
  specialInstructions: string;
}

interface CartContextType {
  cart: CartItem[];
  cartId: number | null;
  loading: boolean;
  fetchCart: () => Promise<void>;
  addToCart: (restaurantId: number, menuItemId: number, quantity?: number) => Promise<void>;
  removeFromCart: (restaurantId: number, menuItemId: number) => Promise<void>;
  createOrder: (deliveryInfo: DeliveryInfo) => Promise<any>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

// Base URL cho API
const API_BASE_URL = 'http://localhost:5182';

export const useCart = () => {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be used within CartProvider');
  return ctx;
};

export const CartProvider = ({ children }: { children: ReactNode }) => {
  const [cart, setCart] = useState<CartItem[]>([]);
  const [cartId, setCartId] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);

  const fetchCart = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(`${API_BASE_URL}/api/Cart`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });
      if (!res.ok) throw new Error('Không thể lấy giỏ hàng');
      const data = await res.json();
      setCartId(data.id);
      const cartItems: CartItem[] = data.items.map((item: any) => ({
        id: item.id,
        menuItemId: item.menuItemId,
        name: item.menuItemName,
        price: item.price,
        quantity: item.quantity,
        imageUrl: '',
        restaurantId: data.restaurantId
      }));
      setCart(cartItems);
    } catch (e) {
      console.error('Error fetching cart:', e);
      setCart([]);
      setCartId(null);
    } finally {
      setLoading(false);
    }
  };

  const addToCart = async (restaurantId: number, menuItemId: number, quantity: number = 1) => {
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(`${API_BASE_URL}/api/Cart/restaurant/${restaurantId}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ menuItemId, quantity }),
      });
      if (!res.ok) throw new Error('Không thể thêm vào giỏ hàng');
      await fetchCart();
    } catch (e) {
      console.error('Error adding to cart:', e);
      throw e;
    } finally {
      setLoading(false);
    }
  };

  const removeFromCart = async (restaurantId: number, menuItemId: number) => {
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      // Find the cart item ID from the cart state
      const cartItem = cart.find(item => item.menuItemId === menuItemId);
      if (!cartItem) {
        throw new Error('Không tìm thấy sản phẩm trong giỏ hàng');
      }
      const res = await fetch(`${API_BASE_URL}/api/Cart/items/${cartItem.id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });
      if (!res.ok) {
        const errorText = await res.text();
        throw new Error(errorText || 'Không thể xóa khỏi giỏ hàng');
      }
      await fetchCart();
    } catch (e) {
      console.error('Error removing from cart:', e);
      throw e;
    } finally {
      setLoading(false);
    }
  };

  const createOrder = async (deliveryInfo: DeliveryInfo) => {
    if (!cartId) {
      throw new Error('Không tìm thấy giỏ hàng');
    }
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(`${API_BASE_URL}/api/Orders/from-cart/${cartId}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          phoneNumber: deliveryInfo.phoneNumber,
          deliveryAddress: deliveryInfo.deliveryAddress,
          specialInstructions: deliveryInfo.specialInstructions
        }),
      });
      if (!res.ok) {
        const errorText = await res.text();
        throw new Error(errorText || 'Không thể đặt hàng');
      }
      const orderData = await res.json();
      // Clear cart after successful order
      setCart([]);
      setCartId(null);
      return orderData;
    } catch (e) {
      console.error('Error creating order:', e);
      throw e;
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCart();
  }, []);

  return (
    <CartContext.Provider value={{ cart, cartId, loading, fetchCart, addToCart, removeFromCart, createOrder }}>
      {children}
    </CartContext.Provider>
  );
};