import React, { useEffect, useState } from "react";
import {
  Row,
  Col,
  Input,
  Button,
  Table,
} from "reactstrap";
import { Form } from "react-bootstrap";
import usersAPI from "../../api/usersApi";

const Users = () => {
  const [users, setUsers] = useState([]);
  // Phân trang
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [hasPreviousPage, setHasPreviousPage] = useState(false);

  // Bộ lọc
  const [searchTerm, setSearchTerm] = useState("");
  const [roleFilter, setRoleFilter] = useState("All");
  const [joinedFilter, setJoinedFilter] = useState("Anytime");

  const fetchUsers = async (page, size) => {
    try {
      const response = await usersAPI.getAll(page, size);
      const data = response;
      setUsers(data.items || []);
      setPageNumber(data.pageNumber);
      setPageSize(data.pageSize);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
      setHasNextPage(data.hasNextPage);
      setHasPreviousPage(data.hasPreviousPage);
    } catch (error) {
      console.error("Error fetching users:", error);
    }
  };

  useEffect(() => {
    fetchUsers(pageNumber, pageSize);
  }, [pageNumber, pageSize]);

  // Lọc user trên frontend (demo)
  const filteredUsers = users.filter((u) => {
    const fullName = (u.firstName + " " + u.lastName).toLowerCase();
    const email = (u.email || "").toLowerCase();
    const matchesSearch =
      fullName.includes(searchTerm.toLowerCase()) ||
      email.includes(searchTerm.toLowerCase()) ||
      u.id.toLowerCase().includes(searchTerm.toLowerCase());

    let matchesRole = true;
    if (roleFilter !== "All") {
      matchesRole = u.roles.some((r) => r.toLowerCase() === roleFilter.toLowerCase());
    }

    return matchesSearch && matchesRole;
  });

  const renderRoles = (roles) => {
    if (!roles || roles.length === 0) return "N/A";
    return roles.join(", ");
  };

  return (
    <div style={{ backgroundColor: "#fff", color: "#000", minHeight: "100vh", padding: "20px" }}>
      <h3 style={{ marginBottom: "20px" }}>User Management</h3>

      {/* Thanh công cụ trên 1 hàng */}
      <Row
        style={{
          backgroundColor: "#f5f5f5",
          padding: "15px",
          borderRadius: "8px",
          marginBottom: "20px",
        }}
        className="align-items-center"
      >
        {/* Col trái: Search + Filter */}
        <Col md="8" className="d-flex align-items-center gap-2">
          <Input
            type="text"
            placeholder="Search items..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ backgroundColor: "#fff", color: "#000", border: "1px solid #ccc" }}
          />
          <Form.Select
            style={{ backgroundColor: "#fff", color: "#000", border: "1px solid #ccc", width: "180px" }}
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value)}
          >
            <option value="All">Permissions: All</option>
            <option value="Admin">Admin</option>
            <option value="User">User</option>
          </Form.Select>
          <Form.Select
            style={{ backgroundColor: "#fff", color: "#000", border: "1px solid #ccc", width: "180px" }}
            value={joinedFilter}
            onChange={(e) => setJoinedFilter(e.target.value)}
          >
            <option value="Anytime">Joined: Anytime</option>
            <option value="LastWeek">Last week</option>
            <option value="LastMonth">Last month</option>
          </Form.Select>
        </Col>

        {/* Col phải: Export + New User */}
        <Col md="4" className="d-flex justify-content-end gap-2">
          <Button color="secondary" style={{ border: "none" }}>
            Export
          </Button>
          <Button color="warning" style={{ border: "none" }}>
            New User
          </Button>
        </Col>
      </Row>

      {/* Bảng hiển thị user */}
      <Row>
        <Col>
          <Table responsive style={{ backgroundColor: "#fff", borderRadius: "8px", overflow: "hidden" }}>
            <thead style={{ backgroundColor: "#eee" }}>
              <tr>
                <th><Input type="checkbox" /></th>
                <th>Full Name</th>
                <th>Email</th>
                <th>Phone Number</th>
                <th>FCM Token</th>
                <th>Roles</th>
                <th>ID</th>
              </tr>
            </thead>
            <tbody>
              {filteredUsers.map((user) => {
                const fullName = `${user.firstName} ${user.lastName}`.trim() || "(No name)";
                return (
                  <tr key={user.id}>
                    <td><Input type="checkbox" /></td>
                    <td>{fullName}</td>
                    <td>{user.email || "N/A"}</td>
                    <td>{user.phoneNumber || "N/A"}</td>
                    <td style={{ maxWidth: "150px", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                      {user.fcmToken || "N/A"}
                    </td>
                    <td>{renderRoles(user.roles)}</td>
                    <td>{user.id}</td>
                  </tr>
                );
              })}
              {filteredUsers.length === 0 && (
                <tr>
                  <td colSpan="7" className="text-center">
                    No users found.
                  </td>
                </tr>
              )}
            </tbody>
          </Table>
        </Col>
      </Row>

      {/* Footer phân trang */}
      <Row className="mt-3" style={{ alignItems: "center" }}>
        <Col md="6">
          <span>
            Showing{" "}
            {filteredUsers.length > 0
              ? (pageNumber - 1) * pageSize + 1
              : 0}{" "}
            -{" "}
            {(pageNumber - 1) * pageSize + filteredUsers.length} of{" "}
            {totalCount} results
          </span>
        </Col>
        <Col
          md="6"
          className="d-flex justify-content-end align-items-center"
          style={{ gap: "10px" }}
        >
          <span>Rows:</span>
          <Form.Select
            style={{ width: "80px", backgroundColor: "#fff", color: "#000", border: "1px solid #ccc" }}
            value={pageSize}
            onChange={(e) => {
              setPageSize(parseInt(e.target.value, 10));
              setPageNumber(1);
            }}
          >
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
          </Form.Select>
          <Button
            color="secondary"
            disabled={pageNumber <= 1 || !hasPreviousPage}
            onClick={() => setPageNumber(pageNumber - 1)}
            style={{ border: "none" }}
          >
            Prev
          </Button>
          <Button
            color="secondary"
            disabled={pageNumber >= totalPages || !hasNextPage}
            onClick={() => setPageNumber(pageNumber + 1)}
            style={{ border: "none" }}
          >
            Next
          </Button>
        </Col>
      </Row>
    </div>
  );
};

export default Users;
