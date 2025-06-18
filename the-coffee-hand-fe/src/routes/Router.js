import { lazy, Suspense } from "react";
import { Navigate } from "react-router-dom";
import ProtectedRoute from "../components/ProtectedRoute";

/****Layouts*****/
const FullLayout = lazy(() => import("../layouts/FullLayout.js"));

/***** Pages ****/
const Starter = lazy(() => import("../views/Starter.js"));
const Orders = lazy(() => import("../views/ui/Orders"));
const Payments = lazy(() => import("../views/ui/Payments"));
const Drinks = lazy(() => import("../views/ui/Drinks"));
const Ingredients = lazy(() => import("../views/ui/Ingredients"));
const Recipes = lazy(() => import("../views/ui/Recipes"));
const Users = lazy(() => import("../views/ui/Users"));
const Reports = lazy(() => import("../views/ui/Reports"));
const Settings = lazy(() => import("../views/ui/Settings"));
const Test = lazy(() => import("../views/ui/Sample/Tables"));
const Login = lazy(() => import("../views/ui/login/index"));

// Preload components
const preloadComponents = () => {
  const components = [
    Starter,
    Orders,
    Payments,
    Drinks,
    Ingredients,
    Recipes,
    Users,
    Reports,
    Settings,
    Test,
    Login
  ];
  
  components.forEach(component => {
    if (component.preload) {
      component.preload();
    }
  });
};

/*****Routes******/
const ThemeRoutes = [
  { 
    path: "/login", 
    exact: true, 
    element: (
      <Suspense fallback={<div>Loading...</div>}>
        <Login />
      </Suspense>
    )
  },

  {
    path: "/",
    element: (
      <ProtectedRoute>
        <Suspense fallback={<div>Loading...</div>}>
          <FullLayout />
        </Suspense>
      </ProtectedRoute>
    ),
    children: [
      { 
        path: "/", 
        element: <Navigate to="/orders" /> 
      },
      { 
        path: "/starter", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Starter />
          </Suspense>
        )
      },
      { 
        path: "/orders", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Orders />
          </Suspense>
        )
      },
      { 
        path: "/payments", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Payments />
          </Suspense>
        )
      },
      { 
        path: "/drinks", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Drinks />
          </Suspense>
        )
      },
      { 
        path: "/ingredients", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Ingredients />
          </Suspense>
        )
      },
      { 
        path: "/recipes", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Recipes />
          </Suspense>
        )
      },
      { 
        path: "/users", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Users />
          </Suspense>
        )
      },
      { 
        path: "/reports", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Reports />
          </Suspense>
        )
      },
      { 
        path: "/machines", 
        exact: true, 
        element: (
          <Suspense fallback={<div>Loading...</div>}>
            <Settings />
          </Suspense>
        )
      },     
    ],
  },
];

export default ThemeRoutes;
