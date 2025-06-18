import { Card, CardBody, CardTitle, CardSubtitle, Table } from "reactstrap";

const DynamicProjectTable = ({
  title = "Project Listing",
  subtitle = "Overview of the projects",
  columns,
  colSizes,
  data
}) => {
  if (!columns || !colSizes || columns.length !== colSizes.length) {
    console.error("columns và colSizes phải có cùng kích thước!");
    return null;
  }

  const totalColSize = colSizes.reduce((acc, size) => acc + size, 0);
  if (totalColSize !== 12) {
    console.warn("Tổng các phần tử trong colSizes phải bằng 12.");
  }

  return (
    <Card>
      <CardBody>
        <CardTitle tag="h5">{title}</CardTitle>
        <CardSubtitle className="mb-2 text-muted" tag="h6">
          {subtitle}
        </CardSubtitle>

        <Table className="no-wrap mt-3 align-middle" responsive borderless>
          <thead>
            <tr>
              {columns.map((col, index) => (
                <th
                  key={index}
                  style={{ width: `${(colSizes[index] / 12) * 100}%` }}
                >
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((row, rowIndex) => (
              <tr key={rowIndex} className="border-top">
                {columns.map((col, colIndex) => (
                  <td
                    key={colIndex}
                    style={{ width: `${(colSizes[colIndex] / 12) * 100}%` }}
                  >
                    <div style={{ margin: "10px 0" }}>
                        {col.render ? col.render(row) : row[col.key]}   
                    </div>             
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </Table>
      </CardBody>
    </Card>
  );
};

export default DynamicProjectTable;
