# Order Service
## Project structure
- [API Gateway](https://github.com/mibrgmv/order-service-gateway/)
- [Order Creation Service](https://github.com/mibrgmv/order-creation-service/)
- Order Processing Service (Work in progress...)
### gRPC Gateway
A gateway service that exposes an API and handles user requests.
### Order Creation Service
A service (gRPC server) that is responsible for order creation and completion. It communicates with **Order Processing Service** via 2 Kafka topics.
