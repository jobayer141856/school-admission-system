import { createBrowserRouter } from 'react-router-dom';
import { Layout } from '../components/layout/Layout';
import { AdmissionsPage } from '../features/admission-classes/pages/AdmissionsPage';
import { MyApplicationsPage } from '../features/student-holds/pages/MyApplicationsPage';
import { AdminPage } from '../features/admin/pages/AdminPage';
import { NotFoundPage } from '../pages/NotFoundPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <Layout>
        <AdmissionsPage />
      </Layout>
    ),
  },
  {
    path: '/my-applications',
    element: (
      <Layout>
        <MyApplicationsPage />
      </Layout>
    ),
  },
  {
    path: '/admin',
    element: (
      <Layout>
        <AdminPage />
      </Layout>
    ),
  },
  {
    path: '*',
    element: (
      <Layout>
        <NotFoundPage />
      </Layout>
    ),
  },
]);

