# CashFlowControl.Solution

[![CI Pipeline](https://github.com/mgracietti/CashFlowControl.Solution/actions/workflows/ci.yml/badge.svg)](https://github.com/mgracietti/CashFlowControl.Solution/actions/workflows/ci.yml)

## Introduction
CashFlowControl.Solution is a comprehensive financial management application designed to streamline cash flow tracking, transaction management, and daily financial summaries. This solution employs clean architecture principles, ensuring maintainability, scalability, and testability.

## Project Structure
The project is structured following the Clean Architecture approach, which separates concerns and facilitates the addition of new features and services.

### Key Components:
- **Core**: Contains the business logic and domain entities.
- **Application**: Includes the services and application logic.
- **Infrastructure**: Handles data access, external services, and other infrastructure concerns.
- **Presentation**: Manages the user interface and API endpoints.

### Clear Architecture - Horizontal Layer View
![Untitled Diagram drawio](https://github.com/user-attachments/assets/c13ddb59-6f5e-44cb-9920-2d17e4bafeef)

![1Untitled Diagram drawio](https://github.com/user-attachments/assets/07d2b792-fe91-4293-98b8-912b1e0a3235)

### Cloud Architecture (AWS)
![2banco-carrefour drawio](https://github.com/user-attachments/assets/982b5ad7-b5de-4588-929f-24c4acdaf87c)

### Components:

1. **Enterprise Apps, Web Apps, Mobile Apps, Real-time Dashboards, IoT Devices**:
   - These are the client-side applications interacting with your AWS infrastructure over HTTPS.

2. **Route 53**:
   - AWS's DNS service, used to route end-user requests to the appropriate services.

3. **ACM (AWS Certificate Manager)**:
   - Manages SSL/TLS certificates for securing communications.

4. **WAF (Web Application Firewall)**:
   - Protects web applications from common web exploits.

5. **Amazon API Gateway**:
   - Acts as a front door for applications to access data, business logic, or functionality from backend services.

6. **AWS Cognito**:
   - Manages authentication, authorization, and user management.

7. **ECS Fargate**:
   - A serverless compute engine for containers, allowing you to run containers without managing servers or clusters.

8. **Elastic Container Registry (ECR)**:
   - Stores, manages, and deploys Docker container images.

9. **Elastic Load Balancer**:
   - Distributes incoming application traffic across multiple targets (e.g., EC2 instances, containers).

10. **VPC (Virtual Private Cloud)**:
    - Provides isolated network environments for your AWS resources.
    - Contains public and private subnets.

11. **NAT Gateway**:
    - Enables instances in a private subnet to connect to the internet while preventing the internet from initiating a connection with those instances.

12. **Internet Gateway**:
    - Connects the VPC to the internet.

13. **MySQL (RDS) Databases**:
    - Managed relational database service for MySQL.
    - Includes write and read replicas for high availability and load balancing.

14. **ElasticCache (Redis)**:
    - Managed Redis service for caching to improve application performance.

15. **AWS Systems Manager and Parameter Store**:
    - Provides operational insights and a secure key/value store to manage configurations.

16. **Monitoring Tools**:
    - **Amazon CloudWatch**: Monitors and logs your AWS resources and applications.
    - **AWS CloudTrail**: Tracks user activity and API usage.
    - **AWS X-Ray**: Traces and analyzes requests to identify performance bottlenecks.
    - **Amazon Elasticsearch Service**: Managed service to deploy, operate, and scale Elasticsearch for log and search analytics.

### How They Interact:

- **Client Interaction**:
  - Clients (enterprise apps, web apps, mobile apps, etc.) interact with the system via HTTPS.
  - Requests are routed through **Route 53** and secured using **ACM**.

- **API Gateway & WAF**:
  - **Amazon API Gateway** manages the API calls and interacts with the **Web Application Firewall** to filter out malicious traffic.
  - **AWS Cognito** handles user authentication and authorization.

- **Compute & Container Management**:
  - **ECS Fargate** runs containerized applications without managing the underlying infrastructure.
  - Docker images are stored and pulled from **ECR**.

- **Load Balancing**:
  - The **Elastic Load Balancer** distributes incoming traffic to ECS tasks (containers).

- **Networking**:
  - The architecture includes a **VPC** with private and public subnets.
  - The **NAT Gateway** allows private subnet resources to access the internet securely.
  - The **Internet Gateway** provides internet access to the VPC.

- **Data Layer**:
  - The **MySQL (RDS) databases** handle the data storage with read replicas for load balancing.
  - **ElasticCache (Redis)** is used for caching to reduce database load and improve performance.

- **Security & Management**:
  - **AWS Systems Manager** and **Parameter Store** are used for operational insights and managing configurations securely.

- **Monitoring**:
  - **Amazon CloudWatch**, **AWS CloudTrail**, **AWS X-Ray**, and **Amazon Elasticsearch Service** provide comprehensive monitoring, logging, and tracing capabilities to ensure the system's reliability and performance.

### Example Terraform Configuration:

Here's a simplified example of how you could define some of these resources using Terraform:

```hcl
provider "aws" {
  region = "us-east-1"
}

resource "aws_vpc" "main" {
  cidr_block = "10.0.0.0/16"
}

resource "aws_subnet" "public" {
  vpc_id            = aws_vpc.main.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "us-east-1a"
  map_public_ip_on_launch = true
}

resource "aws_subnet" "private" {
  vpc_id            = aws_vpc.main.id
  cidr_block        = "10.0.2.0/24"
  availability_zone = "us-east-1a"
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.main.id
}

resource "aws_nat_gateway" "nat" {
  allocation_id = aws_eip.nat.id
  subnet_id     = aws_subnet.public.id
}

resource "aws_eip" "nat" {
  vpc = true
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }
}

resource "aws_route_table_association" "public" {
  subnet_id      = aws_subnet.public.id
  route_table_id = aws_route_table.public.id
}

resource "aws_ecs_cluster" "main" {
  name = "main-cluster"
}

resource "aws_ecs_task_definition" "app" {
  family                = "app"
  network_mode          = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                   = 256
  memory                = 512

  container_definitions = jsonencode([
    {
      name  = "app"
      image = "YOUR_ECR_IMAGE_URI"
      essential = true
      portMappings = [
        {
          containerPort = 80
          hostPort      = 80
        }
      ]
    }
  ])
}

resource "aws_ecs_service" "app" {
  name            = "app-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app.arn
  desired_count   = 2

  network_configuration {
    subnets = [aws_subnet.private.id]
    security_groups = [aws_security_group.main.id]
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.app.arn
    container_name   = "app"
    container_port   = 80
  }
}

resource "aws_lb" "app" {
  name               = "app-lb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.lb.id]
  subnets            = [aws_subnet.public.id]
}

resource "aws_lb_target_group" "app" {
  name     = "app-tg"
  port     = 80
  protocol = "HTTP"
  vpc_id   = aws_vpc.main.id
}

resource "aws_security_group" "main" {
  vpc_id = aws_vpc.main.id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_security_group" "lb" {
  vpc_id = aws_vpc.main.id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}
```


## Technologies Used
- **ASP.NET Core**: For building web APIs.
- **Entity Framework Core**: For database management and ORM.
- **xUnit**: For unit testing.
- **Moq**: For mocking dependencies in tests.
- **Docker**: For containerization.
- **Swagger**: For API documentation.

## Installation and Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)

### Steps to Run the Application
1. **Clone the Repository**
   ```bash
   git clone https://github.com/mgracietti/CashFlowControl.Solution.git      
   ```
2. **Run setup script**

   *Linux*
   ```bash
   cd CashFlowControl.Solution/scripts   
   ./run.sh
   ```
   *Windows*
   ```bash
   cd CashFlowControl.Solution/scripts   
   ./run.ps1
   ```

2. **Nativatge to API Documentation**

In your browser access the API documentation and explore the available endpoints.
- TransactionsService: [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)
- DailySummaryService: [http://localhost:8081/swagger/index.html]( http://localhost:8081/swagger/index.html)

### Usage Examples
1. **Adding a Transaction**
```bash
curl -X POST "http://localhost:8080/api/transactions" -H "Content-Type: application/json" -d '{
  "amount": 100,
  "date": "2024-08-05T04:55:56Z",
  "isCredit": true,
  "description": "Payment received"
}'

```


2. **Getting Daily Summary**
```bash
curl -X GET "http://localhost:8080/api/dailysummary?date=2024-08-05"
```
