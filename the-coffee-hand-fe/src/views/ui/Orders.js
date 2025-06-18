import React, { useEffect, useState } from "react";
import {
  Row,
  Col,
  Input,
  Button,
  Nav,
  NavItem,
  NavLink,
  Label,
  FormGroup,
} from "reactstrap";
import { Form } from "react-bootstrap";
import classnames from "classnames"; // Nếu bạn muốn toggle class active cho NavLink
import orderAPI from "../../api/orderApi"; // API orders
import drinksAPI from "../../api/drinksApi"; // API drinks
import usersAPI from "../../api/usersApi"; // API users
import DynamicProjectTable from "../../components/dashboard/DynamicTable";

const Orders = () => {
  const [orders, setOrders] = useState([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [hasPreviousPage, setHasPreviousPage] = useState(false);

  // Các state lọc dữ liệu
  const [userId, setUserId] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [statusTab, setStatusTab] = useState("all");
  const [error, setError] = useState("");

  const [drinksMap, setDrinksMap] = useState({});
  const [usersMap, setUsersMap] = useState({});
  const [isLoading, setIsLoading] = useState(false);

  // Format date to MM/dd/yyyy
  const formatDate = (date) => {
    if (!date) return "";
    const d = new Date(date);
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');
    const year = d.getFullYear();
    return `${month}/${day}/${year}`;
  };

  // Lấy danh sách orders với filter
  const fetchOrders = async () => {
    setIsLoading(true);
    setError("");
    try {
      const formattedStartDate = startDate ? formatDate(startDate) : "";
      const formattedEndDate = endDate ? formatDate(endDate) : "";
      const status = statusTab === "all" ? "" : statusTab === "process" ? "Pending" : "Done";

      const response = await orderAPI.getAll(
        pageNumber,
        pageSize,
        userId,
        formattedStartDate,
        formattedEndDate,
        status
      );
      
      const data = response;
      console.log(data);
      setOrders(data.items || []);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
      setHasNextPage(data.hasNextPage);
      setHasPreviousPage(data.hasPreviousPage);
    } catch (error) {
      console.error("Lỗi khi lấy danh sách orders:", error);
      setError("Không thể tải danh sách orders. Vui lòng thử lại sau.");
    } finally {
      setIsLoading(false);
    }
  };

  // Lấy danh sách drinks
  const fetchDrinks = async () => {
    try {
      const response = await drinksAPI.getAll(1, 100);
      const drinksArray = response.items || [];
      
      const mapping = {};
      drinksArray.forEach((drink) => {
        mapping[drink.id] = drink.name;
      });
      setDrinksMap(mapping);
    } catch (error) {
      console.error("Lỗi khi lấy danh sách drinks:", error);
    }
  };

  // Effect cho việc load dữ liệu
  useEffect(() => {
    fetchOrders();
  }, [pageNumber, pageSize, userId, startDate, endDate, statusTab]);

  useEffect(() => {
    fetchDrinks();
  }, []);

  // Load user info
  useEffect(() => {
    orders.forEach((order) => {
      if (!usersMap[order.userId]) {
        usersAPI
          .getUsersById(order.userId)
          .then((response) => {
            const user = response;
            const fullName = `${user.email}`;
            setUsersMap((prev) => ({ ...prev, [order.userId]: fullName }));
          })
          .catch((error) => {
            console.error("Lỗi khi lấy thông tin user:", error);
          });
      }
    });
  }, [orders]);

  // Remove the filteredOrders variable since we're now filtering at the API level
  const filteredOrders = orders;

  // Handle filter changes
  const handleFilterChange = (type, value) => {
    setPageNumber(1); // Reset về trang 1 khi filter thay đổi
    switch(type) {
      case 'userId':
        setUserId(value);
        break;
      case 'startDate':
        setStartDate(value);
        break;
      case 'endDate':
        setEndDate(value);
        break;
      default:
        break;
    }
  };

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
      {/* Thanh filter */}
      <Row className="align-items-center mb-3">
        <Col md={3}>
          <FormGroup>
            <Label for="userId">User ID</Label>
            <Input
              id="userId"
              type="text"
              placeholder="Enter User ID"
              value={userId}
              onChange={(e) => handleFilterChange('userId', e.target.value)}
            />
          </FormGroup>
        </Col>
        <Col md={3}>
          <FormGroup>
            <Label for="startDate">From</Label>
            <Input
              id="startDate"
              type="date"
              value={startDate}
              onChange={(e) => handleFilterChange('startDate', e.target.value)}
            />
          </FormGroup>
        </Col>
        {startDate && (
          <Col md={3}>
            <FormGroup>
              <Label for="endDate">To</Label>
              <Input
                id="endDate"
                type="date"
                value={endDate}
                min={startDate}
                onChange={(e) => handleFilterChange('endDate', e.target.value)}
              />
            </FormGroup>
          </Col>
        )}
      </Row>

      {/* Hiển thị lỗi nếu có */}
      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      {/* Page Size Dropdown */}
      <Row className="mb-3">
        <Col md={3}>
          <FormGroup>
            <Label for="pageSize">Orders per page</Label>
            <Input
              id="pageSize"
              type="select"
              value={pageSize}
              onChange={(e) => setPageSize(Number(e.target.value))}
            >
              <option value={5}>5 orders</option>
              <option value={10}>10 orders</option>
              <option value={20}>20 orders</option>
              <option value={50}>50 orders</option>
            </Input>
          </FormGroup>
        </Col>
      </Row>

      {/* Status tabs */}
      <Nav tabs className="mb-3">
        <NavItem>
          <NavLink
            className={classnames({ active: statusTab === "all" })}
            onClick={() => setStatusTab("all")}
            style={{ cursor: "pointer" }}
          >
            All
          </NavLink>
        </NavItem>
        <NavItem>
          <NavLink
            className={classnames({ active: statusTab === "process" })}
            onClick={() => setStatusTab("process")}
            style={{ cursor: "pointer" }}
          >
            Process
          </NavLink>
        </NavItem>
        <NavItem>
          <NavLink
            className={classnames({ active: statusTab === "finish" })}
            onClick={() => setStatusTab("finish")}
            style={{ cursor: "pointer" }}
          >
            Finish
          </NavLink>
        </NavItem>
      </Nav>

      {/* Loading indicator */}
      {isLoading && (
        <div className="text-center my-3">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      )}

      {/* Bảng dữ liệu */}
      <Row>
        <Col lg="12">
          <DynamicProjectTable
            data={filteredOrders}
            columns={columns}
            colSizes={colSizes}
          />
        </Col>
      </Row>

      {/* Phân trang */}
      <Row className="mt-3">
        <Col className="d-flex align-items-center justify-content-between">
          <div className="d-flex align-items-center">
            <span>Total: {totalCount} orders</span>
          </div>
          <div>
            <Button
              color="primary"
              className="ms-2"
              disabled={!hasPreviousPage || isLoading}
              onClick={() => setPageNumber(pageNumber - 1)}
            >
              Prev
            </Button>
            <span className="mx-3">
              Page {pageNumber} of {totalPages}
            </span>
            <Button
              color="primary"
              className="ms-2"
              disabled={!hasNextPage || isLoading}
              onClick={() => setPageNumber(pageNumber + 1)}
            >
              Next
            </Button>
          </div>
        </Col>
      </Row>
    </div>
  );
};

export default Orders;
