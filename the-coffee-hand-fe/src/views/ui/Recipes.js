import React, { useState, useEffect } from "react";
import {
  Row,
  Col,
  Card,
  CardBody,
  CardTitle,
  Table,
  Badge,
  Spinner,
  Alert,
  Button,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Form,
  FormGroup,
  Label,
  Input
} from "reactstrap";
import { recipeAPI } from "../../api/recipeApi";
import drinksAPI from "../../api/drinksApi";
import machineAPI from "../../api/machineApi";
import ingredientsAPI from "../../api/ingredientsApi";

const Recipes = () => {
  const [drinks, setDrinks] = useState([]);
  const [recipes, setRecipes] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // State for recipe modal
  const [modal, setModal] = useState(false);
  const [selectedDrink, setSelectedDrink] = useState(null);
  const [machines, setMachines] = useState([]);
  const [ingredients, setIngredients] = useState([]);
  const [machineInfo, setMachineInfo] = useState([]);
  const [currentRecipe, setCurrentRecipe] = useState({
    drinkName: "",
    recipeSteps: []
  });
  
  // State for current step
  const [currentStep, setCurrentStep] = useState({
    ingredient: "",
    machineName: "",
    action: "",
    parametersRequired: []
  });

  // State for machine parameters
  const [selectedMachine, setSelectedMachine] = useState(null);
  const [machineActions, setMachineActions] = useState([]);
  const [actionParameters, setActionParameters] = useState([]);

  // Reset form state
  const resetForm = () => {
    setCurrentStep({
      ingredient: "",
      machineName: "",
      action: "",
      parametersRequired: []
    });
    setSelectedMachine(null);
    setMachineActions([]);
    setActionParameters([]);
  };

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const [drinksRes, recipesRes, machinesRes, ingredientsRes] = await Promise.all([
        drinksAPI.getAll(1, 100),
        recipeAPI.getAll(),
        machineAPI.getAll(),
        ingredientsAPI.getAll(1, 100),
      ]);

      console.log('Drinks response:', drinksRes);
      console.log('Recipes response:', recipesRes);
      console.log('Machines response:', machinesRes);
      console.log('Ingredients response:', ingredientsRes);

      setDrinks(drinksRes.items || []);
      setRecipes(Array.isArray(recipesRes) ? recipesRes : []);
      setMachines(machinesRes || []);
      setIngredients(ingredientsRes.items || []);
      setMachineInfo(machinesRes || []);
    } catch (err) {
      console.error("Error fetching data:", err);
      setError("Failed to fetch data");
    } finally {
      setIsLoading(false);
    }
  };

  const handleIngredientChange = (ingredientName) => {
    console.log('Selected ingredient:', ingredientName);
    console.log('Machine info:', machineInfo);
    const machineForIngredient = machineInfo.find(m => m.ingredient.toLowerCase() === ingredientName.toLowerCase());
    console.log('Machine info for ingredient:', machineForIngredient);

    if (machineForIngredient) {
      setSelectedMachine(machineForIngredient);
      console.log('Selected machine:',selectedMachine)
      setMachineActions(machineForIngredient.parameter || []);
      setCurrentStep(prev => ({
        ...prev,
        ingredient: ingredientName,
        machineName: machineForIngredient.machineName,
        action: "",
        parametersRequired: []
      }));
    } else {
      // Reset states if no machine found
      setSelectedMachine(null);
      setMachineActions([]);
      setCurrentStep(prev => ({
        ...prev,
        ingredient: ingredientName,
        machineName: "",
        action: "",
        parametersRequired: []
      }));
    }
  };

  const handleActionChange = (mode) => {
  if (!selectedMachine) return;

  const selectedMode = selectedMachine.parameters.find(action => action.mode === mode); 
  console.log('Selected mode:', selectedMode);

  if (selectedMode) {
    setActionParameters(selectedMode.parametersRequired || []); // Đúng tên biến là 'parametersRequired'
    setCurrentStep(prev => ({
      ...prev,
      action: mode,
      parametersRequired: selectedMode.parametersRequired.map(param => ({
        name: param,
        value: 0
      }))
    }));
  }
};

  const handleParameterChange = (paramName, value) => {
    setCurrentStep(prev => ({
      ...prev,
      parametersRequired: prev.parametersRequired.map(param =>
        param.name === paramName ? { ...param, value: Number(value) || 0 } : param
      )
    }));
  };

  const addStep = () => {
    if (!currentStep.ingredient || !currentStep.machineName || !currentStep.action) {
      return;
    }
    
    setCurrentRecipe(prev => ({
      ...prev,
      recipeSteps: [...prev.recipeSteps, currentStep]
    }));
    
    setSelectedMachine(null)
    resetForm();
  };

  const removeStep = (index) => {
    setCurrentRecipe(prev => ({
      ...prev,
      recipeSteps: prev.recipeSteps.filter((_, i) => i !== index)
    }));
  };

  const handleDrinkClick = (drink) => {
    console.log('Selected drink:', drink);
    console.log('Current recipes:', recipes);
    
    const recipe = recipes.find(r => r.drinkName?.toLowerCase() === drink.name?.toLowerCase());
    console.log('Found recipe:', recipe);
    
    setSelectedDrink(drink);
    setCurrentRecipe(recipe || {
      drinkName: drink.name,
      recipeSteps: []
    });
    resetForm();
    setModal(true);
  };

  const handleDeleteRecipe = async () => {
    try {
      if (!selectedDrink) return;
      
      // Here you would call your API to delete the recipe
      // await recipeAPI.deleteRecipe(selectedDrink.name);
      
      setCurrentRecipe({
        drinkName: selectedDrink.name,
        recipeSteps: []
      });
      setModal(false);
      setSelectedDrink(null);
      await fetchData(); // Refresh the data
    } catch (err) {
      console.error("Error deleting recipe:", err);
      setError("Failed to delete recipe");
    }
  };

  const handleSubmit = async () => {
    try {
      if (!currentRecipe.drinkName || !Array.isArray(currentRecipe.recipeSteps)) {
        setError("Invalid recipe data");
        return;
      }

      console.log('Submitting recipe:', currentRecipe);

      // First, create the recipe steps
      try {
        const recipeStepData = {
          id: selectedDrink.id,
          drinkName: currentRecipe.drinkName,
          recipeSteps: currentRecipe.recipeSteps
        };

        await recipeAPI.createRecipeStep(selectedDrink.id, recipeStepData);
        console.log('Successfully created recipe steps');
      } catch (error) {
        console.error('Error creating recipe steps:', error);
        throw error;
      }

      // Create temporary array to store volume-related information
      const volumeSteps = [];
      
      // Process each step sequentially using for...of to allow await
      for (const step of currentRecipe.recipeSteps) {
        for (const param of step.parametersRequired) {
          if (param.name.toLowerCase() === 'volumn' || param.name.toLowerCase() === 'quantity') {
            try {
              // Get ingredient ID by name
              const ingredientResponse = await ingredientsAPI.getIngredientByName(step.ingredient);
              console.log(ingredientResponse)
              
              volumeSteps.push({
                selectedDrink: selectedDrink.id,
                ingredient: ingredientResponse.id, // Use the ingredient ID instead of name
                quantity: param.value
              });
            } catch (error) {
              console.error(`Error fetching ingredient ID for ${step.ingredient}:`, error);
              // Continue with other steps even if one fails
            }
          }
        }
      }

      console.log('Steps with volume parameter:', volumeSteps);

      // Create recipes with volume data
      try {
        for (const volumeStep of volumeSteps) {
          await recipeAPI.createRecipeWithVolume({
            quantity: volumeStep.quantity,
            ingredientId: volumeStep.ingredient,
            drinkId: volumeStep.selectedDrink
          });
        }
        console.log('Successfully created all recipes with volume');
      } catch (error) {
        console.error('Error creating recipes with volume:', error);
        throw error; // This will be caught by the outer try-catch
      }

      setModal(false);
      setSelectedDrink(null);
      setCurrentRecipe({
        drinkName: "",
        recipeSteps: []
      });
      
      // Refresh the data after creating new recipe
      await fetchData();
    } catch (err) {
      console.error("Error saving recipe:", err);
      setError("Failed to save recipe");
    }
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '100vh' }}>
        <Spinner color="primary" />
      </div>
    );
  }

  if (error) {
    return (
      <Alert color="danger">
        {error}
      </Alert>
    );
  }

  return (
    <div className="p-4">
      <h2 className="mb-4">Recipes Management</h2>
      
      <Row>
        {drinks.map((drink) => {
          const hasRecipe = recipes.some(r => r.drinkName.toLowerCase() === drink.name.toLowerCase());
          
          return (
            <Col key={drink.id} md="3" className="mb-4">
              <Card 
                onClick={() => handleDrinkClick(drink)}
                style={{ 
                  cursor: 'pointer',
                  height: '320px' // Fixed height for all cards
                }}
              >
                <img
                  src={drink.imageUrl || "/logo192.png"}
                  alt={drink.name}
                  style={{
                    width: "100%",
                    height: "200px",
                    objectFit: "cover"
                  }}
                  onError={(e) => {
                    e.target.onerror = null;
                    e.target.src = "/logo192.png";
                  }}
                />
                <CardBody style={{ height: '120px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                  <CardTitle tag="h5" className="mb-2">{drink.name}</CardTitle>
                  {!hasRecipe && (
                    <Alert color="warning" className="mb-0 py-1">
                      This drink doesn't have a recipe
                    </Alert>
                  )}
                  {hasRecipe && (
                    <div className="text-success mb-0 py-1">
                      Recipe available
                    </div>
                  )}
                </CardBody>
              </Card>
            </Col>
          );
        })}
      </Row>

      {/* Recipe Modal */}
      <Modal isOpen={modal} toggle={() => setModal(false)} size="lg">
        <ModalHeader toggle={() => setModal(false)}>
          {selectedDrink ? `Recipe for ${selectedDrink.name}` : 'Create Recipe'}
        </ModalHeader>
        <ModalBody>
          {/* Current Steps */}
          <div className="mb-4">
            <h5>Recipe Steps</h5>
            {currentRecipe.recipeSteps.length > 0 ? (
              <Table>
                <thead>
                  <tr>
                    <th>Step</th>
                    <th>Ingredient</th>
                    <th>Machine</th>
                    <th>Action</th>
                    <th>Parameters</th>
                  </tr>
                </thead>
                <tbody>
                  {currentRecipe.recipeSteps.map((step, index) => (
                    <tr key={index}>
                      <td>{index + 1}</td>
                      <td>{step.ingredient}</td>
                      <td>{step.machineName}</td>
                      <td>{step.action}</td>
                      <td>
                        {step.parametersRequired.map((param, i) => (
                          <Badge key={i} color="info" className="me-1">
                            {param.name}: {param.value}
                          </Badge>
                        ))}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            ) : (
              <Alert color="info">
                No steps added yet. Use the form below to add steps to the recipe.
              </Alert>
            )}
          </div>

          {/* Add New Step Form - Only show when there are no steps */}
          {/* {currentRecipe.recipeSteps.length === 0 && ( */}
            <div className="border p-3">
              <h5>Add New Step</h5>
              <Form>
                <Row>
                  <Col md={6}>
                    <FormGroup>
                      <Label>Ingredient</Label>
                      <Input
                        type="select"
                        value={currentStep.ingredient || ""}
                        onChange={(e) => handleIngredientChange(e.target.value)}
                      >
                        <option value="">Select ingredient...</option>
                        {ingredients.map(ing => (
                          <option key={ing.id} value={ing.name}>
                            {ing.name}
                          </option>
                        ))}
                      </Input>
                    </FormGroup>
                  </Col>
                  <Col md={6}>
                    <FormGroup>
                      <Label>Machine</Label>
                      <Input
                        type="text"
                        value={selectedMachine?.machineName || ''}
                        disabled
                        placeholder="Machine"
                      />
                    </FormGroup>
                  </Col>
                </Row>

                {selectedMachine && (
                  <Row>
                    <Col md={6}>
                      <FormGroup>
                        <Label>Action</Label>
                        <Input
                          type="select"
                          value={currentStep.action || ""}
                          onChange={(e) => handleActionChange(e.target.value)}
                        >
                          <option value="">Select action...</option>
                          {selectedMachine.parameters.map((param) => (
                            <option key={param.mode} value={param.mode}>
                              {param.mode}
                            </option>
                          ))}
                        </Input>
                      </FormGroup>
                    </Col>
                  </Row>
                )}

                {actionParameters.map((param, index) => (
                  <Col md={6} key={index}>
                    <FormGroup>
                      <Label>{param}</Label>
                      <Input
                        type="number"
                        value={currentStep.parametersRequired.find(p => p.name === param)?.value || 0}
                        onChange={(e) => handleParameterChange(param, e.target.value)}
                      />
                    </FormGroup>
                  </Col>
                ))}

                <Button 
                  color="primary" 
                  onClick={addStep}
                  disabled={!currentStep.ingredient || !currentStep.machineName || !currentStep.action}
                >
                  Add Step
                </Button>
              </Form>
            </div>
          {/* )} */}
        </ModalBody>
        <ModalFooter>
          <Button color="danger" onClick={handleDeleteRecipe}>
            Delete Recipe
          </Button>
          <Button color="secondary" onClick={() => setModal(false)}>
            Cancel
          </Button>
          <Button
            color="primary"
            onClick={handleSubmit}
            disabled={currentRecipe.recipeSteps.length === 0}
          >
            Save Recipe
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};

export default Recipes;
