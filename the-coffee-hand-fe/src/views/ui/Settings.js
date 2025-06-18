import {
  Row,
  Col,
  Table,
  Input,
  Spinner,
  Badge
} from "reactstrap";
import { useState, useEffect } from "react";
import machineAPI from "../../api/machineApi";

const Settings = () => {
  const [machines, setMachines] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const fetchMachines = async () => {
    setIsLoading(true);
    try {
      console.log('Fetching machines...');
      const response = await machineAPI.getAll();
      console.log('API Response:', response);
      
      if (!response) {
        console.error('No response from API');
        return;
      }


      setMachines(response);
    } catch (error) {
      console.error("Error fetching machines:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchMachines();
  }, []);

  const filteredMachines = machines.filter((machine) => {
    if (!machine || !machine.machineName) return false;
    const lowerName = machine.machineName.toLowerCase();
    const lowerSearch = searchTerm.toLowerCase();
    return lowerName.includes(lowerSearch);
  });

  // Function to get mode color
  const getModeColor = (mode) => {
    const colors = {
      grind: 'primary',
      brew: 'success',
      heat: 'danger',
      pump: 'warning',
      whisk: 'info',
      adding: 'secondary',
      heating: 'danger'
    };
    return colors[mode] || 'primary';
  };

  return (
    <div style={{ backgroundColor: "#fff", color: "#000", minHeight: "100vh", padding: "20px" }}>
      <h3 style={{ marginBottom: "20px" }}>Coffee Machine Management</h3>

      <Row
        style={{
          backgroundColor: "#f5f5f5",
          padding: "15px",
          borderRadius: "8px",
          marginBottom: "20px",
        }}
        className="align-items-center"
      >
        <Col className="d-flex align-items-center gap-2">
          <Input
            type="text"
            placeholder="Search machines..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ backgroundColor: "#fff", color: "#000", border: "1px solid #ccc" }}
          />
        </Col>
      </Row>

      <Row>
        <Col>
          <Table responsive style={{ backgroundColor: "#fff", borderRadius: "8px", overflow: "hidden" }}>
            <thead style={{ backgroundColor: "#eee" }}>
              <tr>
                <th>Machine Name</th>
                <th>Ingredient</th>
                <th>Modes</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan="3" className="text-center py-5">
                    <Spinner color="primary" />
                  </td>
                </tr>
              ) : (
                <>
                  {filteredMachines.map((machine) => (
                    <tr key={machine.id}>
                      <td>{machine.machineName}</td>
                      <td>{machine.ingredient}</td>
                      <td>
                        {machine.parameters?.map((param) => (
                          <div 
                            key={param.mode} 
                            style={{
                              marginBottom: "8px",
                              padding: "8px",
                              backgroundColor: "#f8f9fa",
                              borderRadius: "4px",
                              border: "1px solid #dee2e6"
                            }}
                          >
                            <Badge 
                              color={getModeColor(param.mode)}
                              style={{ 
                                marginRight: "8px",
                                textTransform: "uppercase",
                                fontSize: "0.8em",
                                padding: "5px 8px"
                              }}
                            >
                              {param.mode}
                            </Badge>
                            <span style={{ color: "#666", fontSize: "0.9em" }}>
                              {param.parametersRequired?.join(" • ")}
                            </span>
                          </div>
                        )) || 
                        <div className="text-muted">No parameters</div>}
                      </td>
                    </tr>
                  ))}
                  {filteredMachines.length === 0 && !isLoading && (
                    <tr>
                      <td colSpan="3" className="text-center">
                        No machines found.
                      </td>
                    </tr>
                  )}
                </>
              )}
            </tbody>
          </Table>
        </Col>
      </Row>
    </div>
  );
};

export default Settings;
