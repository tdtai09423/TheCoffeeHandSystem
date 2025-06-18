import React from "react";
import { useRoutes } from "react-router-dom";

// Hàm bọc element với Layout (nếu có)
const wrapWithLayout = (element, Layout) => {
  if (Layout === null || Layout === undefined) {
    return element;
  }
  const LayoutComponent = Layout;
  return <LayoutComponent>{element}</LayoutComponent>;
};

// Hàm đệ quy để biến đổi cấu hình routes
const transformRoutes = (routes, parentLayout = null) => {
  return routes.map((route) => {
    // Sử dụng layout của route nếu có, ngược lại kế thừa từ cha
    const currentLayout = route.hasOwnProperty("layout") ? route.layout : parentLayout;

    let newRoute = { ...route };

    if (newRoute.children) {
      newRoute.children = transformRoutes(newRoute.children, currentLayout);
    }
    if (newRoute.element) {
      newRoute.element = wrapWithLayout(newRoute.element, currentLayout);
    }
    return newRoute;
  });
};

export const useGenerateRoutes = (routesConfig) => {
  const transformedRoutes = transformRoutes(routesConfig);
  return useRoutes(transformedRoutes);
};
