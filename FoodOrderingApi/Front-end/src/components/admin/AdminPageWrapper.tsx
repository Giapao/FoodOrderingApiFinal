import React from 'react';
import AdminLayout from './AdminLayout';

interface AdminPageWrapperProps {
  loading: boolean;
  error: string | null;
  children: React.ReactNode;
}

const AdminPageWrapper: React.FC<AdminPageWrapperProps> = ({ loading, error, children }) => {
  if (loading) {
    return <AdminLayout><div className="flex justify-center items-center h-64">Loading...</div></AdminLayout>;
  }

  if (error) {
    return <AdminLayout><div className="text-red-500 p-4">{error}</div></AdminLayout>;
  }

  return <AdminLayout>{children}</AdminLayout>;
};

export default AdminPageWrapper; 