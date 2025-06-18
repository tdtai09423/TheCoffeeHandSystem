import { Nav, NavItem } from "reactstrap";
import { Link, useLocation } from "react-router-dom";
import user1 from "../assets/images/users/user4.jpg";
import './sideBar.scss';

const navigation = [
  // {
  //   title: "Dashboard",
  //   href: "/starter",
  //   icon: "bi bi-speedometer2",
  // },
  {
    title: "Orders",
    href: "/orders",
    icon: "bi bi-cart2",
  },
  {
    title: "Recipes",
    href: "/recipes",
    icon: "bi bi-list-check",
  },
  {
    title: "Drinks & Categories",
    href: "/drinks",
    icon: "bi bi-cup-hot",
  },
  {
    title: "Ingredients",
    href: "/ingredients",
    icon: "bi bi-egg",
  },
  {
    title: "Users Management",
    href: "/users",
    icon: "bi bi-people",
  },
  {
    title: "Statistics & Reports",
    href: "/reports",
    icon: "bi bi-graph-up-arrow",
  },
  {
    title: "Machines",
    href: "/machines",
    icon: "bi bi-gear",
  },
];

const Sidebar = () => {
  let location = useLocation();

  return (
    <div>
      <div className="d-flex align-items-center"></div>
      <div
      >
        <div className="p-3 d-flex justify-content-center align-items-center">
          <img src={user1} alt="user" width="50" className="rounded-circle" />
        </div>
        <div className="bg-dark text-white p-2 opacity-75 text-center">Administrator</div>
      </div>
      <div className="p-3 mt-2">
        <Nav vertical className="sidebarNav">
          {navigation.map((navi, index) => (
            <NavItem key={index} className="sidenav-bg">
              <Link
                to={navi.href}
                className={
                  location.pathname === navi.href
                    ? "active nav-link py-3"
                    : "nav-link text-secondary py-3"
                }
              >
                <i className={navi.icon}></i>
                <span className="ms-3 d-inline-block">{navi.title}</span>
              </Link>
            </NavItem>
          ))}
        </Nav>
      </div>
    </div>
  );
};

export default Sidebar;





// const navigation = [
//   {
//     title: "Dashboard",
//     href: "/starter",
//     icon: "bi bi-speedometer2",
//   },
//   {
//     title: "Alert",
//     href: "/alerts",
//     icon: "bi bi-bell",
//   },
//   {
//     title: "Badges",
//     href: "/badges",
//     icon: "bi bi-patch-check",
//   },
//   {
//     title: "Buttons",
//     href: "/buttons",
//     icon: "bi bi-hdd-stack",
//   },
//   {
//     title: "Cards",
//     href: "/cards",
//     icon: "bi bi-card-text",
//   },
//   {
//     title: "Grid",
//     href: "/grid",
//     icon: "bi bi-columns",
//   },
//   {
//     title: "Table",
//     href: "/table",
//     icon: "bi bi-layout-split",
//   },
//   {
//     title: "Forms",
//     href: "/forms",
//     icon: "bi bi-textarea-resize",
//   },
//   {
//     title: "Breadcrumbs",
//     href: "/breadcrumbs",
//     icon: "bi bi-link",
//   },
// ];