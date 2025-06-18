import React, { useState, useEffect } from "react";
import {
  Row,
  Col,
  Input,
  Button,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  FormGroup,
  Label,
  Spinner,
} from "reactstrap";
import categoriesAPI from "../../api/categoriesApi";
import drinksAPI from "../../api/drinksApi";

const Drinks = () => {
  // Danh sách categories và drinks từ API
  const [categories, setCategories] = useState([]);
  const [drinks, setDrinks] = useState([]);
  const [activeCategoryId, setActiveCategoryId] = useState("all");

  // State cho thêm mới category
  const [newCategoryName, setNewCategoryName] = useState("");
  const [isAddingCategory, setIsAddingCategory] = useState(false);

  // State cho modal thêm mới Drink
  const [isAddingDrink, setIsAddingDrink] = useState(false);
  const [newDrink, setNewDrink] = useState({
    name: "",
    description: "",
    price: "",
    categoryId: "",
    isAvailable: true,
    imageUrl: "",
  });

  // State hiển thị thông tin drink (modal chi tiết drink)
  const [selectedDrink, setSelectedDrink] = useState(null);

  // State cho modal xác nhận xóa (dùng chung cho category và drink)
  const [deleteConfirmModal, setDeleteConfirmModal] = useState({
    isOpen: false,
    type: null, // "category" hoặc "drink"
    id: null,
    name: "",
  });

  // State cho việc cập nhật drink
  const [updatedDrink, setUpdatedDrink] = useState(null);
  const [isFormChanged, setIsFormChanged] = useState(false);

  // State cho loading
  const [isLoading, setIsLoading] = useState(true);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isAdding, setIsAdding] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // State cho upload ảnh
  const [isUploading, setIsUploading] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  // Lấy danh sách categories và drinks từ API
  const fetchData = async () => {
    setIsLoading(true);
    try {
      const [categoriesRes, drinksRes] = await Promise.all([
        categoriesAPI.getAll(1, 100),
        drinksAPI.getAll(1, 100)
      ]);
      
      setCategories([{ id: "all", name: "All Drinks" }, ...categoriesRes.items]);
      setDrinks(drinksRes.items);
    } catch (error) {
      console.error("Error fetching data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Xác nhận thêm mới category
  const handleConfirmAddCategory = async () => {
    if (!newCategoryName.trim()) return;
    setIsAdding(true);
    try {
      await categoriesAPI.addNewCategories(newCategoryName);
      await fetchData();
      setNewCategoryName("");
      setIsAddingCategory(false);
    } catch (error) {
      console.error("Error adding category:", error);
    } finally {
      setIsAdding(false);
    }
  };

  // Hủy việc thêm mới category
  const handleCancelAddCategory = () => {
    setNewCategoryName("");
    setIsAddingCategory(false);
  };

  // Lọc drinks theo category đang chọn
  const displayedDrinks =
    activeCategoryId === "all"
      ? drinks
      : drinks.filter(
          (drink) =>
            drink.categoryId?.toLowerCase() === activeCategoryId.toLowerCase()
        );

  // Xử lý thay đổi input trong modal thêm Drink
  const handleNewDrinkChange = (field, value) => {
    setNewDrink((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  // Hàm log thông tin drink mới
  const logNewDrinkInfo = (drink) => {
    console.log("Thông tin drink mới sẽ được thêm:", drink);
  };

  // Validate form
  const validateForm = (drink) => {
    const errors = {};
    if (!drink.name.trim()) errors.name = "Name is required";
    if (!drink.price) errors.price = "Price is required";
    if (!drink.categoryId) errors.categoryId = "Category is required";
    if (!drink.description.trim()) errors.description = "Description is required";
    return errors;
  };

  // Xử lý upload ảnh
  const handleImageUpload = async (event, isUpdating = false) => {
    const file = event.target.files[0];
    if (!file) return;

    setIsUploading(true);
    try {
      const response = await drinksAPI.uploadImage(file);
      if (isUpdating) {
        handleUpdateDrinkChange("imageUrl", response.data.url);
      } else {
        handleNewDrinkChange("imageUrl", response.data.url);
      }
    } catch (error) {
      console.error("Error uploading image:", error);
    } finally {
      setIsUploading(false);
    }
  };

  // Xác nhận thêm Drink
  const handleConfirmAddDrink = async () => {
    const errors = validateForm(newDrink);
    setFormErrors(errors);
    if (Object.keys(errors).length > 0) return;

    setIsAdding(true);
    try {
      await drinksAPI.addNewDrinks({
        ...newDrink,
        price: parseFloat(newDrink.price),
      });
      await fetchData();
      setIsAddingDrink(false);
      setNewDrink({
        name: "",
        description: "",
        price: "",
        categoryId: "",
        isAvailable: true,
        imageUrl: "",
      });
      setFormErrors({});
    } catch (error) {
      console.error("Error adding drink:", error);
    } finally {
      setIsAdding(false);
    }
  };

  // Mở modal xác nhận xóa cho category
  const openDeleteConfirmModalForCategory = (category) => {
    setDeleteConfirmModal({
      isOpen: true,
      type: "category",
      id: category.id,
      name: category.name,
    });
  };

  // Mở modal xác nhận xóa cho drink
  const openDeleteConfirmModalForDrink = (drink) => {
    setDeleteConfirmModal({
      isOpen: true,
      type: "drink",
      id: drink.id,
      name: drink.name,
    });
  };

  // Xử lý xác nhận xóa
  const handleConfirmDelete = async () => {
    const { type, id } = deleteConfirmModal;
    setIsDeleting(true);
    try {
      if (type === "category") {
        await categoriesAPI.deleteCategoriesById(id);
        if (activeCategoryId.toLowerCase() === id.toLowerCase()) {
          setActiveCategoryId("all");
        }
      } else if (type === "drink") {
        await drinksAPI.deleteDrinksById(id);
        setSelectedDrink(null);
      }
      await fetchData();
    } catch (error) {
      console.error("Error deleting:", error);
    } finally {
      setIsDeleting(false);
      setDeleteConfirmModal({ isOpen: false, type: null, id: null, name: "" });
    }
  };

  // Xử lý khi mở modal chi tiết drink
  const handleOpenDrinkDetail = (drink) => {
    setSelectedDrink(drink);
    setUpdatedDrink({ ...drink });
    setIsFormChanged(false);
  };

  // Xử lý thay đổi thông tin drink trong modal chi tiết
  const handleUpdateDrinkChange = (field, value) => {
    setUpdatedDrink(prev => ({
      ...prev,
      [field]: value
    }));
    setIsFormChanged(true);
  };

  // Xử lý cập nhật thông tin drink
  const handleUpdateDrink = async () => {
    const errors = validateForm(updatedDrink);
    setFormErrors(errors);
    if (Object.keys(errors).length > 0) return;

    setIsUpdating(true);
    try {
      await drinksAPI.updateDrink(selectedDrink.id, {
        ...updatedDrink,
        price: parseFloat(updatedDrink.price)
      });
      await fetchData();
      setSelectedDrink(null);
      setUpdatedDrink(null);
      setIsFormChanged(false);
      setFormErrors({});
    } catch (error) {
      console.error("Error updating drink:", error);
    } finally {
      setIsUpdating(false);
    }
  };

  return (
    <Row style={{ minHeight: "100vh" }}>
      {isLoading ? (
        <Col className="text-center p-5">
          <Spinner color="primary" />
          <p className="mt-2">Loading...</p>
        </Col>
      ) : (
        <>
          {/* Sidebar Categories */}
          <Col
            md="3"
            style={{
              backgroundColor: "#f5f5f5",
              padding: "20px",
              borderRight: "1px solid #ccc",
            }}
          >
            <h5>Categories</h5>
            <ul
              style={{
                listStyleType: "none",
                paddingLeft: 0,
                marginTop: "1rem",
              }}
            >
              {categories.map((cat) => (
                <li
                  key={cat.id}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    marginBottom: "10px",
                    cursor: "pointer",
                    padding: "5px 10px",
                    borderRadius: "4px",
                    fontWeight: activeCategoryId === cat.id ? "bold" : "normal",
                    color: activeCategoryId === cat.id ? "#fff" : "#000",
                    backgroundColor:
                      activeCategoryId === cat.id ? "#000" : "transparent",
                  }}
                >
                  <span onClick={() => setActiveCategoryId(cat.id)}>
                    {cat.name}
                  </span>
                  {/* Nếu không phải "All Drinks", hiển thị nút xóa */}
                  {cat.id !== "all" && (
                    <Button
                      color="danger"
                      size="sm"
                      onClick={(e) => {
                        e.stopPropagation();
                        openDeleteConfirmModalForCategory(cat);
                      }}
                    >
                      Delete
                    </Button>
                  )}
                </li>
              ))}

              {/* Phần thêm mới category */}
              {isAddingCategory ? (
                <li
                  style={{
                    marginBottom: "10px",
                    padding: "5px 10px",
                    borderRadius: "4px",
                    backgroundColor: "#e0e0e0",
                  }}
                >
                  <Input
                    type="text"
                    value={newCategoryName}
                    onChange={(e) => setNewCategoryName(e.target.value)}
                    placeholder="New category name"
                    disabled={isAdding}
                  />
                  <div
                    style={{
                      marginTop: "5px",
                      display: "flex",
                      gap: "5px",
                    }}
                  >
                    <Button
                      color="primary"
                      size="sm"
                      onClick={handleConfirmAddCategory}
                      disabled={isAdding}
                    >
                      {isAdding ? <Spinner size="sm" /> : "Confirm"}
                    </Button>
                    <Button
                      color="secondary"
                      size="sm"
                      onClick={handleCancelAddCategory}
                      disabled={isAdding}
                    >
                      Cancel
                    </Button>
                  </div>
                </li>
              ) : (
                <li
                  style={{
                    marginBottom: "10px",
                    cursor: "pointer",
                    padding: "5px 10px",
                    borderRadius: "4px",
                    fontWeight: "normal",
                    color: "#000",
                    backgroundColor: "transparent",
                    border: "1px dashed #ccc",
                  }}
                  onClick={() => setIsAddingCategory(true)}
                >
                  + Add new category
                </li>
              )}
            </ul>
          </Col>

          {/* Main Content - Drinks */}
          <Col md="9" style={{ padding: "20px" }}>
            {/* Header Row: Button Add New Drink và Title */}
            <Row
              style={{
                display: "flex",
                alignItems: "center",
                marginBottom: "1rem",
              }}
            >
              <Col md="3" style={{ textAlign: "left" }}>
                <h4 style={{ margin: 0 }}>
                  {activeCategoryId === "all"
                    ? "All Drinks"
                    : categories.find(
                        (c) =>
                          c.id.toLowerCase() === activeCategoryId.toLowerCase()
                      )?.name || "Category"}
                </h4>
              </Col>
              <Col md="9" style={{ textAlign: "right" }}>
                <Button
                  color="primary"
                  onClick={() => setIsAddingDrink(true)}
                  style={{ marginRight: "1rem" }}
                  disabled={isAdding}
                >
                  {isAdding ? (
                    <>
                      <Spinner size="sm" /> Adding...
                    </>
                  ) : (
                    "Add New Drink"
                  )}
                </Button>
              </Col>
            </Row>

            {/* Danh sách drinks */}
            <Row
              style={{
                marginTop: "1rem",
                display: "flex",
                flexWrap: "wrap",
              }}
            >
              {displayedDrinks.map((drink) => (
                <div
                  key={drink.id}
                  style={{
                    flex: "0 0 20%",
                    maxWidth: "20%",
                    padding: "10px",
                    boxSizing: "border-box",
                  }}
                >
                  <div
                    style={{
                      border: "1px solid #ccc",
                      borderRadius: "8px",
                      overflow: "hidden",
                      textAlign: "center",
                      boxShadow: "0 2px 5px rgba(0,0,0,0.1)",
                      cursor: "pointer",
                    }}
                    onClick={() => handleOpenDrinkDetail(drink)}
                  >
                    <img
                      src={drink.imageUrl || "/logo192.png"}
                      alt={drink.name}
                      style={{
                        width: "100%",
                        height: "200px",
                        objectFit: "cover",
                      }}
                      onError={(e) => {
                        e.target.onerror = null;
                        e.target.src = "/logo192.png";
                      }}
                    />
                    <div style={{ padding: "10px" }}>
                      <h5 style={{ fontSize: "1rem", margin: "0.5rem 0" }}>
                        {drink.name}
                      </h5>
                      <p style={{ margin: 0, color: "#555" }}>
                        {drink.price ? `$${drink.price}` : "N/A"}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </Row>
          </Col>
        </>
      )}

      {/* Modal thêm mới Drink */}
      <Modal
        isOpen={isAddingDrink}
        toggle={() => !isAdding && setIsAddingDrink(false)}
      >
        <ModalHeader toggle={() => !isAdding && setIsAddingDrink(false)}>
          Add New Drink
        </ModalHeader>
        <ModalBody>
          <FormGroup>
            <Label for="drinkName">Name *</Label>
            <Input
              id="drinkName"
              type="text"
              value={newDrink.name}
              onChange={(e) => handleNewDrinkChange("name", e.target.value)}
              placeholder="Enter drink name"
              invalid={!!formErrors.name}
            />
            {formErrors.name && (
              <div className="text-danger small">{formErrors.name}</div>
            )}
          </FormGroup>
          <FormGroup>
            <Label for="drinkDescription">Description *</Label>
            <Input
              id="drinkDescription"
              type="textarea"
              value={newDrink.description}
              onChange={(e) => handleNewDrinkChange("description", e.target.value)}
              placeholder="Enter drink description"
              invalid={!!formErrors.description}
            />
            {formErrors.description && (
              <div className="text-danger small">{formErrors.description}</div>
            )}
          </FormGroup>
          <FormGroup>
            <Label for="drinkPrice">Price *</Label>
            <Input
              id="drinkPrice"
              type="number"
              value={newDrink.price}
              onChange={(e) => handleNewDrinkChange("price", e.target.value)}
              placeholder="Enter drink price"
              invalid={!!formErrors.price}
            />
            {formErrors.price && (
              <div className="text-danger small">{formErrors.price}</div>
            )}
          </FormGroup>
          <FormGroup>
            <Label for="drinkImage">Image</Label>
            <div className="d-flex align-items-center gap-2">
              {newDrink.imageUrl && (
                <img
                  src={newDrink.imageUrl}
                  alt="Preview"
                  style={{ width: "50px", height: "50px", objectFit: "cover" }}
                />
              )}
              <Input
                id="drinkImage"
                type="file"
                accept="image/*"
                onChange={(e) => handleImageUpload(e, false)}
                disabled={isUploading}
              />
              {isUploading && <Spinner size="sm" />}
            </div>
          </FormGroup>
          <FormGroup>
            <Label for="drinkCategory">Category *</Label>
            <Input
              id="drinkCategory"
              type="select"
              value={newDrink.categoryId}
              onChange={(e) => handleNewDrinkChange("categoryId", e.target.value)}
              invalid={!!formErrors.categoryId}
            >
              <option value="">Select a category</option>
              {categories
                .filter((cat) => cat.id !== "all")
                .map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
            </Input>
            {formErrors.categoryId && (
              <div className="text-danger small">{formErrors.categoryId}</div>
            )}
          </FormGroup>
          <FormGroup check>
            <Label check>
              <Input
                type="checkbox"
                checked={newDrink.isAvailable}
                onChange={(e) => handleNewDrinkChange("isAvailable", e.target.checked)}
              />{" "}
              Is Available
            </Label>
          </FormGroup>
        </ModalBody>
        <ModalFooter>
          <Button 
            color="primary" 
            onClick={handleConfirmAddDrink}
            disabled={isAdding || isUploading}
          >
            {isAdding ? <Spinner size="sm" /> : "Confirm"}
          </Button>
          <Button 
            color="secondary" 
            onClick={() => {
              setIsAddingDrink(false);
              setFormErrors({});
            }}
            disabled={isAdding}
          >
            Cancel
          </Button>
        </ModalFooter>
      </Modal>

      {/* Modal hiển thị thông tin drink */}
      {selectedDrink && (
        <Modal 
          isOpen={true} 
          toggle={() => !isUpdating && setSelectedDrink(null)} 
          size="lg"
        >
          <ModalHeader toggle={() => !isUpdating && setSelectedDrink(null)}>
            Edit Drink: {selectedDrink.name}
          </ModalHeader>
          <ModalBody>
            <FormGroup>
              <Label for="editDrinkName">Name *</Label>
              <Input
                id="editDrinkName"
                type="text"
                value={updatedDrink?.name || ""}
                onChange={(e) => handleUpdateDrinkChange("name", e.target.value)}
                placeholder="Enter drink name"
                invalid={!!formErrors.name}
              />
              {formErrors.name && (
                <div className="text-danger small">{formErrors.name}</div>
              )}
            </FormGroup>
            <FormGroup>
              <Label for="editDrinkDescription">Description *</Label>
              <Input
                id="editDrinkDescription"
                type="textarea"
                value={updatedDrink?.description || ""}
                onChange={(e) => handleUpdateDrinkChange("description", e.target.value)}
                placeholder="Enter drink description"
                invalid={!!formErrors.description}
              />
              {formErrors.description && (
                <div className="text-danger small">{formErrors.description}</div>
              )}
            </FormGroup>
            <FormGroup>
              <Label for="editDrinkPrice">Price *</Label>
              <Input
                id="editDrinkPrice"
                type="number"
                value={updatedDrink?.price || ""}
                onChange={(e) => handleUpdateDrinkChange("price", e.target.value)}
                placeholder="Enter drink price"
                invalid={!!formErrors.price}
              />
              {formErrors.price && (
                <div className="text-danger small">{formErrors.price}</div>
              )}
            </FormGroup>
            <FormGroup>
              <Label for="editDrinkImage">Image</Label>
              <div className="d-flex align-items-center gap-2">
                {updatedDrink?.imageUrl && (
                  <img
                    src={updatedDrink.imageUrl}
                    alt="Preview"
                    style={{ width: "50px", height: "50px", objectFit: "cover" }}
                  />
                )}
                <Input
                  id="editDrinkImage"
                  type="file"
                  accept="image/*"
                  onChange={(e) => handleImageUpload(e, true)}
                  disabled={isUploading}
                />
                {isUploading && <Spinner size="sm" />}
              </div>
            </FormGroup>
            <FormGroup>
              <Label for="editDrinkCategory">Category *</Label>
              <Input
                id="editDrinkCategory"
                type="select"
                value={updatedDrink?.categoryId || ""}
                onChange={(e) => handleUpdateDrinkChange("categoryId", e.target.value)}
                invalid={!!formErrors.categoryId}
              >
                <option value="">Select a category</option>
                {categories
                  .filter((cat) => cat.id !== "all")
                  .map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.name}
                    </option>
                  ))}
              </Input>
              {formErrors.categoryId && (
                <div className="text-danger small">{formErrors.categoryId}</div>
              )}
            </FormGroup>
            <FormGroup check>
              <Label check>
                <Input
                  type="checkbox"
                  checked={updatedDrink?.isAvailable || false}
                  onChange={(e) => handleUpdateDrinkChange("isAvailable", e.target.checked)}
                />{" "}
                Is Available
              </Label>
            </FormGroup>
          </ModalBody>
          <ModalFooter style={{ justifyContent: "flex-start" }}>
            <Button 
              color="primary" 
              onClick={handleUpdateDrink}
              disabled={!isFormChanged || isUpdating || isUploading}
            >
              {isUpdating ? <Spinner size="sm" /> : "Update"}
            </Button>
            <Button 
              color="secondary" 
              onClick={() => {
                setSelectedDrink(null);
                setFormErrors({});
              }}
              disabled={isUpdating}
            >
              Cancel
            </Button>
            <Button
              color="danger"
              onClick={() => openDeleteConfirmModalForDrink(selectedDrink)}
              disabled={isUpdating}
            >
              Delete
            </Button>
          </ModalFooter>
        </Modal>
      )}

      {/* Modal xác nhận xóa */}
      {deleteConfirmModal.isOpen && (
        <Modal
          isOpen={deleteConfirmModal.isOpen}
          toggle={() => !isDeleting && setDeleteConfirmModal({
            ...deleteConfirmModal,
            isOpen: false,
          })}
        >
          <ModalHeader
            toggle={() => !isDeleting && setDeleteConfirmModal({
              ...deleteConfirmModal,
              isOpen: false,
            })}
          >
            Confirm Deletion
          </ModalHeader>
          <ModalBody>
            Are you sure you want to delete "{deleteConfirmModal.name}"?
          </ModalBody>
          <ModalFooter>
            <Button
              color="secondary"
              onClick={() =>
                setDeleteConfirmModal({
                  ...deleteConfirmModal,
                  isOpen: false,
                })
              }
              disabled={isDeleting}
            >
              Cancel
            </Button>
            <Button 
              color="danger" 
              onClick={handleConfirmDelete}
              disabled={isDeleting}
            >
              {isDeleting ? <Spinner size="sm" /> : "Delete"}
            </Button>
          </ModalFooter>
        </Modal>
      )}
    </Row>
  );
};

export default Drinks;
