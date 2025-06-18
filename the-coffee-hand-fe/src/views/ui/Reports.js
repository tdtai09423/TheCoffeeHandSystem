import {
  Row,
  Col,
  Card,
  CardBody,
  CardTitle,
  Nav,
  NavItem,
  NavLink,
  Input,
  FormGroup,
  Label,
  Spinner
} from "reactstrap";
import { useState, useEffect } from "react";
import { Line, Bar } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import orderAPI from "../../api/orderApi";
import classnames from "classnames";

// Đăng ký các components cần thiết cho Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend
);

const Reports = () => {
  const [activeTab, setActiveTab] = useState('month');
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [selectedMonth, setSelectedMonth] = useState(new Date().getMonth() + 1);
  const [isLoading, setIsLoading] = useState(false);
  const [monthlyData, setMonthlyData] = useState([]);
  const [yearlyData, setYearlyData] = useState([]);
  const [currentStats, setCurrentStats] = useState({
    totalRevenue: 0,
    orderCount: 0,
    averageOrderValue: 0
  });
  const [previousStats, setPreviousStats] = useState({
    totalRevenue: 0,
    orderCount: 0,
    averageOrderValue: 0
  });

  // Tạo danh sách năm cho select (từ 2020 đến năm hiện tại)
  const years = Array.from(
    { length: new Date().getFullYear() - 2020 + 1 },
    (_, i) => 2020 + i
  );

  // Tạo danh sách tháng cho select
  const months = Array.from({ length: 12 }, (_, i) => i + 1);

  // Hàm tính toán thống kê từ dữ liệu orders
  const calculateStats = (orders) => {
    const totalRevenue = orders.reduce((sum, order) => sum + (order.totalPrice || 0), 0);
    const orderCount = orders.length;
    const averageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0;

    return {
      totalRevenue,
      orderCount,
      averageOrderValue
    };
  };

  // Hàm tính phần trăm thay đổi
  const calculateChange = (current, previous) => {
    if (previous === 0) return 100;
    return ((current - previous) / previous) * 100;
  };

  // Hàm lấy dữ liệu theo tháng
  const fetchMonthlyData = async () => {
    setIsLoading(true);
    try {
      // Lấy dữ liệu tháng hiện tại
      const startDate = new Date(selectedYear, selectedMonth - 1, 1);
      const endDate = new Date(selectedYear, selectedMonth, 0);
      
      const response = await orderAPI.getAll(
        1,
        1000,
        "",
        startDate.toISOString(),
        endDate.toISOString()
      );

      // Tạo object để nhóm doanh thu theo ngày
      const dailyRevenue = {};
      const daysInMonth = endDate.getDate();

      // Khởi tạo tất cả các ngày trong tháng với doanh thu 0
      for (let i = 1; i <= daysInMonth; i++) {
        dailyRevenue[i] = 0;
      }

      // Tính tổng doanh thu cho mỗi ngày
      response.items.forEach(order => {
        const orderDate = new Date(order.date);
        const day = orderDate.getDate();
        dailyRevenue[day] += order.totalPrice || 0;
      });

      setMonthlyData(Object.values(dailyRevenue));
      setCurrentStats(calculateStats(response.items));

      // Lấy dữ liệu tháng trước
      const previousStartDate = new Date(selectedYear, selectedMonth - 2, 1);
      const previousEndDate = new Date(selectedYear, selectedMonth - 1, 0);
      
      const previousResponse = await orderAPI.getAll(
        1,
        1000,
        "",
        previousStartDate.toISOString(),
        previousEndDate.toISOString()
      );

      setPreviousStats(calculateStats(previousResponse.items));

    } catch (error) {
      console.error("Error fetching monthly data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Hàm lấy dữ liệu theo năm
  const fetchYearlyData = async () => {
    setIsLoading(true);
    try {
      // Lấy dữ liệu năm hiện tại
      const startDate = new Date(selectedYear, 0, 1);
      const endDate = new Date(selectedYear, 11, 31);
      
      const response = await orderAPI.getAll(
        1,
        1000,
        "",
        startDate.toISOString(),
        endDate.toISOString()
      );

      // Tạo object để nhóm doanh thu theo tháng
      const monthlyRevenue = Array(12).fill(0);

      // Tính tổng doanh thu cho mỗi tháng
      response.items.forEach(order => {
        const orderDate = new Date(order.date);
        const month = orderDate.getMonth();
        monthlyRevenue[month] += order.totalPrice || 0;
      });

      setYearlyData(monthlyRevenue);
      setCurrentStats(calculateStats(response.items));

      // Lấy dữ liệu năm trước
      const previousStartDate = new Date(selectedYear - 1, 0, 1);
      const previousEndDate = new Date(selectedYear - 1, 11, 31);
      
      const previousResponse = await orderAPI.getAll(
        1,
        1000,
        "",
        previousStartDate.toISOString(),
        previousEndDate.toISOString()
      );

      setPreviousStats(calculateStats(previousResponse.items));

    } catch (error) {
      console.error("Error fetching yearly data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (activeTab === 'month') {
      fetchMonthlyData();
    } else {
      fetchYearlyData();
    }
  }, [activeTab, selectedYear, selectedMonth]);

  // Cấu hình cho biểu đồ đường (theo tháng)
  const monthlyChartData = {
    labels: Array.from({ length: monthlyData.length }, (_, i) => i + 1),
    datasets: [
      {
        label: 'Daily Revenue',
        data: monthlyData,
        fill: false,
        borderColor: 'rgb(75, 192, 192)',
        tension: 0.1
      }
    ]
  };

  // Cấu hình cho biểu đồ cột (theo năm)
  const yearlyChartData = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
    datasets: [
      {
        label: 'Monthly Revenue',
        data: yearlyData,
        backgroundColor: 'rgba(75, 192, 192, 0.5)',
        borderColor: 'rgb(75, 192, 192)',
        borderWidth: 1
      }
    ]
  };

  const chartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
      },
      title: {
        display: true,
        text: activeTab === 'month' ? 'Daily Revenue' : 'Monthly Revenue'
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: function(value) {
            return '$' + value.toLocaleString();
          }
        }
      }
    }
  };

  // Format số tiền
  const formatCurrency = (value) => {
    return '$' + value.toLocaleString(undefined, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  };

  return (
    <div>
      <Card>
        <CardBody>
          <CardTitle tag="h5">Revenue Reports</CardTitle>
          
          <Nav tabs className="mb-3">
            <NavItem>
              <NavLink
                className={classnames({ active: activeTab === 'month' })}
                onClick={() => setActiveTab('month')}
                style={{ cursor: 'pointer' }}
              >
                Monthly Report
              </NavLink>
            </NavItem>
            <NavItem>
              <NavLink
                className={classnames({ active: activeTab === 'year' })}
                onClick={() => setActiveTab('year')}
                style={{ cursor: 'pointer' }}
              >
                Yearly Report
              </NavLink>
            </NavItem>
          </Nav>

          <Row className="mb-4">
            <Col md={activeTab === 'month' ? 6 : 12}>
              <FormGroup>
                <Label for="yearSelect">Year</Label>
                <Input
                  type="select"
                  id="yearSelect"
                  value={selectedYear}
                  onChange={(e) => setSelectedYear(parseInt(e.target.value))}
                >
                  {years.map(year => (
                    <option key={year} value={year}>{year}</option>
                  ))}
                </Input>
              </FormGroup>
            </Col>
            {activeTab === 'month' && (
              <Col md={6}>
                <FormGroup>
                  <Label for="monthSelect">Month</Label>
                  <Input
                    type="select"
                    id="monthSelect"
                    value={selectedMonth}
                    onChange={(e) => setSelectedMonth(parseInt(e.target.value))}
                  >
                    {months.map(month => (
                      <option key={month} value={month}>
                        {new Date(2000, month - 1).toLocaleString('default', { month: 'long' })}
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>
            )}
          </Row>

          {/* Overview Statistics */}
          <Row className="mb-4">
            <Col md={4}>
              <Card className="bg-light">
                <CardBody>
                  <h6>Total Revenue</h6>
                  <h3>{formatCurrency(currentStats.totalRevenue)}</h3>
                  <small className={calculateChange(currentStats.totalRevenue, previousStats.totalRevenue) >= 0 ? 'text-success' : 'text-danger'}>
                    {calculateChange(currentStats.totalRevenue, previousStats.totalRevenue).toFixed(1)}% vs previous period
                  </small>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="bg-light">
                <CardBody>
                  <h6>Total Orders</h6>
                  <h3>{currentStats.orderCount}</h3>
                  <small className={calculateChange(currentStats.orderCount, previousStats.orderCount) >= 0 ? 'text-success' : 'text-danger'}>
                    {calculateChange(currentStats.orderCount, previousStats.orderCount).toFixed(1)}% vs previous period
                  </small>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="bg-light">
                <CardBody>
                  <h6>Average Order Value</h6>
                  <h3>{formatCurrency(currentStats.averageOrderValue)}</h3>
                  <small className={calculateChange(currentStats.averageOrderValue, previousStats.averageOrderValue) >= 0 ? 'text-success' : 'text-danger'}>
                    {calculateChange(currentStats.averageOrderValue, previousStats.averageOrderValue).toFixed(1)}% vs previous period
                  </small>
                </CardBody>
              </Card>
            </Col>
          </Row>

          {isLoading ? (
            <div className="text-center py-5">
              <Spinner color="primary" />
            </div>
          ) : (
            <div style={{ height: '400px' }}>
              {activeTab === 'month' ? (
                <Line data={monthlyChartData} options={chartOptions} />
              ) : (
                <Bar data={yearlyChartData} options={chartOptions} />
              )}
            </div>
          )}
        </CardBody>
      </Card>
    </div>
  );
};

export default Reports;
