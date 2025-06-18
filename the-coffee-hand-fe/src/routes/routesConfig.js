// src/routes/routesConfig.js
import { Navigate } from "react-router-dom";
import FullLayout from "../layouts/FullLayout";
import Starter from "../views/Starter";
import Orders from "../views/ui/Orders";
import Payments from "../views/ui/Payments";
import Drinks from "../views/ui/Drinks";
import Ingredients from "../views/ui/Ingredients";
import Recipes from "../views/ui/Recipes";
import Users from "../views/ui/Users";
import Reports from "../views/ui/Reports";
import Settings from "../views/ui/Settings";
import Test from "../views/ui/Sample/Tables";

const routesConfig = [
  {
    path: "/",
    layout: FullLayout,
    children: [
      { path: "/", element: <Navigate to="/starter" /> },
      { path: "/starter", exact: true, element: <Starter />, layout: null },
      { path: "/orders", exact: true, element: <Orders /> },
      { path: "/payments", exact: true, element: <Payments /> },
      { path: "/drinks", exact: true, element: <Drinks /> },
      { path: "/ingredients", exact: true, element: <Ingredients /> },
      { path: "/recipes", exact: true, element: <Recipes /> },
      { path: "/users", exact: true, element: <Users /> },
      { path: "/reports", exact: true, element: <Reports /> },
      { path: "/machines", exact: true, element: <Settings /> },
      { path: "/test", exact: true, element: <Test /> },
    ],
  },
];

export default routesConfig;
