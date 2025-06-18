import { Col, Row } from "reactstrap";
import SalesChart from "../components/dashboard/SalesChart";
import Feeds from "../components/dashboard/Feeds";
import DynamicProjectTable from "../components/dashboard/DynamicTable";
import { useState, useEffect } from "react";
import orderAPI from "../api/orderApi";
import drinksAPI from "../api/drinksApi";
import usersAPI from "../api/usersApi";

const Starter = () => {
  const [orders, setOrders] = useState([]);
  const [drinksMap, setDrinksMap] = useState({});
  const [usersMap, setUsersMap] = useState({});
  const [isLoading, setIsLoading] = useState(true);

  // Lấy danh sách orders
  const fetchOrders = async () => {
    try {
      const response = await orderAPI.getAll(1, 5); // Lấy 5 orders đầu tiên
      const data = response.data;
      setOrders(data.items || []);
    } catch (error) {
      console.error("Error fetching orders:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Lấy danh sách drinks
  const fetchDrinks = async () => {
    try {
      const response = await drinksAPI.getAll(1, 100);
      const drinksArray = response.data.items || [];
      const mapping = {};
      drinksArray.forEach((drink) => {
        mapping[drink.id] = drink.name;
      });
      setDrinksMap(mapping);
    } catch (error) {
      console.error("Error fetching drinks:", error);
    }
  };

  // Load user info
  useEffect(() => {
    orders.forEach((order) => {
      if (!usersMap[order.userId]) {
        usersAPI
          .getUsersById(order.userId)
          .then((response) => {
            const user = response.data;
            const fullName = `${user.firstName} ${user.lastName}`;
            setUsersMap((prev) => ({ ...prev, [order.userId]: fullName }));
          })
          .catch((error) => {
            console.error("Error fetching user info:", error);
          });
      }
    });
  }, [orders]);

  useEffect(() => {
    fetchOrders();
    fetchDrinks();
  }, []);

  // Các cột cho bảng
  const columns = [
    {
      header: "Order ID",
      key: "id",
    },
    {
      header: "Date",
      key: "date",
      render: (row) => {
        const dateObj = new Date(row.date);
        return dateObj.toLocaleString();
      },
    },
    {
      header: "Status",
      key: "status",
      render: (row) => {
        if (row.status === "5") {
          return (
            <span className="p-2 bg-danger rounded-circle d-inline-block ms-3"></span>
          );
        } else if (row.status === "0") {
          return (
            <span className="p-2 bg-warning rounded-circle d-inline-block ms-3"></span>
          );
        } else if (row.status === "4") {
          return (
            <span className="p-2 bg-success rounded-circle d-inline-block ms-3"></span>
          );
        }
        return (
          <span className="p-2 bg-secondary rounded-circle d-inline-block ms-3"></span>
        );
      },
    },
    {
      header: "Total Price",
      key: "totalPrice",
    },
    {
      header: "User",
      key: "userId",
      render: (row) => <div>{usersMap[row.userId] || "Loading..."}</div>,
    },
    {
      header: "Order Details",
      key: "orderDetails",
      render: (row) => (
        <ul style={{ paddingLeft: "1rem" }}>
          {row.orderDetails?.map((detail) => (
            <li key={detail.id}>
              {drinksMap[detail.drinkId] || detail.drinkId} x ({detail.total})
              <div style={{ fontSize: "13px" }}>{detail.note}</div>
            </li>
          ))}
        </ul>
      ),
    },
  ];

  const colSizes = [3, 2, 1, 1, 2, 3];

  return (
    <div>
      {/***Top Cards***/}

      {/***Sales & Feed***/}
      <Row>
        <Col sm="6" lg="6" xl="7" xxl="8">
          <SalesChart />
        </Col>
        <Col sm="6" lg="6" xl="5" xxl="4">
          <Feeds />
        </Col>
      </Row>
      {/***Table ***/}
      <Row>
        <Col lg="12">
          <DynamicProjectTable
            title="Recent Orders"
            subtitle="Latest 5 orders in the system"
            data={orders}
            columns={columns}
            colSizes={colSizes}
          />
        </Col>
      </Row>
    </div>
  );
};

export default Starter;
