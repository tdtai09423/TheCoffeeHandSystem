import {
  Row,
  Col
} from "reactstrap";
import ProjectTables from "../../components/dashboard/ProjectTable";

const Payments = () => {
  return (
    <Row>
      <Col lg="12">
        <ProjectTables />
      </Col>
    </Row>
  );
};

export default Payments;
