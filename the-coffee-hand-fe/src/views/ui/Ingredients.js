import React, { useEffect, useState } from "react";
import { Row, Col, Input, Button, Table, Modal, ModalHeader, ModalBody, ModalFooter, Spinner } from "reactstrap";
import { Form } from "react-bootstrap";
import ingredientsAPI from "../../api/ingredientsApi";

const Ingredients = () => {
  const [ingredients, setIngredients] = useState([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [hasPreviousPage, setHasPreviousPage] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [modal, setModal] = useState(false);
  const [updateModal, setUpdateModal] = useState(false);
  const [deleteModal, setDeleteModal] = useState(false);
  const [selectedIngredient, setSelectedIngredient] = useState(null);
  const [ingredientToDelete, setIngredientToDelete] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [newIngredient, setNewIngredient] = useState({
    name: "",
    quantity: 0,
    price: 0
  });

  const fetchIngredients = async (page, size) => {
    setIsLoading(true);
    try {
      const response = await ingredientsAPI.getAll(page, size);
      const data = response;
      setIngredients(data.items || []);
      setPageNumber(data.pageNumber);
      setPageSize(data.pageSize);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
      setHasNextPage(data.hasNextPage);
      setHasPreviousPage(data.hasPreviousPage);
    } catch (error) {
      console.error("Error fetching ingredients:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchIngredients(pageNumber, pageSize);
  }, [pageNumber, pageSize]);

  const filteredIngredients = ingredients.filter((ing) => {
    const lowerName = ing.name.toLowerCase();
    const lowerSearch = searchTerm.toLowerCase();
    return lowerName.includes(lowerSearch);
  });

  const toggleModal = () => setModal(!modal);
  const toggleUpdateModal = () => setUpdateModal(!updateModal);
  const toggleDeleteModal = () => setDeleteModal(!deleteModal);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setNewIngredient(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleUpdateInputChange = (e) => {
    const { name, value } = e.target;
    setSelectedIngredient(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    try {
      await ingredientsAPI.create(newIngredient);
      toggleModal();
      fetchIngredients(pageNumber, pageSize);
      setNewIngredient({ name: "", quantity: 0, price: 0 });
    } catch (error) {
      console.error("Error creating ingredient:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async () => {
    setIsSubmitting(true);
    try {
      const updateData = {
        id: selectedIngredient.id,
        name: selectedIngredient.name,
        quantity: selectedIngredient.quantity,
        price: selectedIngredient.price
      };
      await ingredientsAPI.update(updateData);
      toggleUpdateModal();
      fetchIngredients(pageNumber, pageSize);
      setSelectedIngredient(null);
    } catch (error) {
      console.error("Error updating ingredient:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (ingredient) => {
    setIngredientToDelete(ingredient);
    toggleDeleteModal();
  };

  const confirmDelete = async () => {
    setIsSubmitting(true);
    try {
      await ingredientsAPI.delete(ingredientToDelete.id);
      toggleDeleteModal();
      fetchIngredients(pageNumber, pageSize);
      setIngredientToDelete(null);
    } catch (error) {
      console.error("Error deleting ingredient:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const openUpdateModal = (ingredient) => {
    setSelectedIngredient({ ...ingredient });
    toggleUpdateModal();
  };

  return (
    <div style={{ backgroundColor: "#fff", color: "#000", minHeight: "100vh", padding: "20px" }}>
      <h3 style={{ marginBottom: "20px" }}>Ingredient Management</h3>

      <Row
        style={{
          backgroundColor: "#f5f5f5",
          padding: "15px",
          borderRadius: "8px",
          marginBottom: "20px",
        }}
        className="align-items-center"
      >
        <Col md="8" className="d-flex align-items-center gap-2">
          <Input
            type="text"
            placeholder="Search ingredients..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ backgroundColor: "#fff", color: "#000", border: "1px solid #ccc" }}
          />
        </Col>
        <Col md="4" className="d-flex justify-content-end gap-2">
          <Button color="warning" style={{ border: "none" }} onClick={toggleModal}>
            New Ingredient
          </Button>
        </Col>
      </Row>

      <Row>
        <Col>
          <Table responsive style={{ backgroundColor: "#fff", borderRadius: "8px", overflow: "hidden" }}>
            <thead style={{ backgroundColor: "#eee" }}>
              <tr>
                <th>
                  <Input type="checkbox" />
                </th>
                <th>Name</th>
                <th>Quantity</th>
                <th>Price</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan="5" className="text-center py-5">
                    <Spinner color="primary" />
                  </td>
                </tr>
              ) : (
                <>
                  {filteredIngredients.map((ing) => (
                    <tr key={ing.id}>
                      <td>
                        <Input type="checkbox" />
                      </td>
                      <td>{ing.name}</td>
                      <td>{ing.quantity}</td>
                      <td>{ing.price}</td>
                      <td>
                        <Button
                          color="primary"
                          size="sm"
                          className="me-2"
                          onClick={() => openUpdateModal(ing)}
                        >
                          Edit
                        </Button>
                        <Button
                          color="danger"
                          size="sm"
                          onClick={() => handleDelete(ing)}
                        >
                          Delete
                        </Button>
                      </td>
                    </tr>
                  ))}
                  {filteredIngredients.length === 0 && !isLoading && (
                    <tr>
                      <td colSpan="5" className="text-center">
                        No ingredients found.
                      </td>
                    </tr>
                  )}
                </>
              )}
            </tbody>
          </Table>
        </Col>
      </Row>

      <Row className="mt-3" style={{ alignItems: "center" }}>
        <Col md="6">
          <span>
            Showing {filteredIngredients.length > 0 ? (pageNumber - 1) * pageSize + 1 : 0} -{" "}
            {(pageNumber - 1) * pageSize + filteredIngredients.length} of {totalCount} results
          </span>
        </Col>
        <Col md="6" className="d-flex justify-content-end align-items-center" style={{ gap: "10px" }}>
          <span>Rows:</span>
          <Form.Select
            style={{ width: "80px", backgroundColor: "#fff", color: "#000", border: "1px solid #ccc" }}
            value={pageSize}
            onChange={(e) => {
              setPageSize(parseInt(e.target.value, 10));
              setPageNumber(1);
            }}
          >
            <option value={10}>10</option>
            <option value={20}>20</option>
          </Form.Select>
          <Button
            color="secondary"
            disabled={pageNumber <= 1 || !hasPreviousPage || isLoading}
            onClick={() => setPageNumber(pageNumber - 1)}
            style={{ border: "none" }}
          >
            Prev
          </Button>
          <Button
            color="secondary"
            disabled={pageNumber >= totalPages || !hasNextPage || isLoading}
            onClick={() => setPageNumber(pageNumber + 1)}
            style={{ border: "none" }}
          >
            Next
          </Button>
        </Col>
      </Row>

      {/* Add New Ingredient Modal */}
      <Modal isOpen={modal} toggle={toggleModal}>
        <ModalHeader toggle={toggleModal}>Add New Ingredient</ModalHeader>
        <ModalBody>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Name</Form.Label>
              <Form.Control
                type="text"
                name="name"
                value={newIngredient.name}
                onChange={handleInputChange}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Quantity</Form.Label>
              <Form.Control
                type="number"
                name="quantity"
                value={newIngredient.quantity}
                onChange={handleInputChange}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Price</Form.Label>
              <Form.Control
                type="number"
                name="price"
                value={newIngredient.price}
                onChange={handleInputChange}
                required
              />
            </Form.Group>
          </Form>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={toggleModal} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button color="primary" onClick={handleSubmit} disabled={isSubmitting}>
            {isSubmitting ? <Spinner size="sm" /> : "Add Ingredient"}
          </Button>
        </ModalFooter>
      </Modal>

      {/* Update Ingredient Modal */}
      <Modal isOpen={updateModal} toggle={toggleUpdateModal}>
        <ModalHeader toggle={toggleUpdateModal}>Update Ingredient</ModalHeader>
        <ModalBody>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Name</Form.Label>
              <Form.Control
                type="text"
                name="name"
                value={selectedIngredient?.name || ""}
                onChange={handleUpdateInputChange}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Quantity</Form.Label>
              <Form.Control
                type="number"
                name="quantity"
                value={selectedIngredient?.quantity || 0}
                onChange={handleUpdateInputChange}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Price</Form.Label>
              <Form.Control
                type="number"
                name="price"
                value={selectedIngredient?.price || 0}
                onChange={handleUpdateInputChange}
                required
              />
            </Form.Group>
          </Form>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={toggleUpdateModal} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button color="primary" onClick={handleUpdate} disabled={isSubmitting}>
            {isSubmitting ? <Spinner size="sm" /> : "Update Ingredient"}
          </Button>
        </ModalFooter>
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal isOpen={deleteModal} toggle={toggleDeleteModal}>
        <ModalHeader toggle={toggleDeleteModal}>Confirm Delete</ModalHeader>
        <ModalBody>
          Are you sure you want to delete ingredient "{ingredientToDelete?.name}"?
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={toggleDeleteModal} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button color="danger" onClick={confirmDelete} disabled={isSubmitting}>
            {isSubmitting ? <Spinner size="sm" /> : "Delete"}
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};

export default Ingredients;
