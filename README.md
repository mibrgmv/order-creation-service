# Order Service
## Project structure
- [API Gateway](https://github.com/mibrgmv/order-service-gateway/)
- [Order Creation Service](https://github.com/mibrgmv/order-creation-service/)
- Order Processing Service (Work in progress...)
### gRPC Gateway
A gateway service that exposes an API and handles user requests.
### Order Creation Service
A service (gRPC server) that is responsible for order creation and completion. It communicates with **Order Processing Service** via 2 Kafka topics.
## Docker support
Here is a docker compose to get up Kafka, Order processing service and its Postgres database. 
```yml
services:
  postgres:
    image: postgres:latest
    container_name: lab-5-tools-postgres
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
      - POSTGRES_DB=postgres
    ports:
      - "5432:5432"
    restart: unless-stopped
    networks:
      - order-processing-service-network
  
  order-processing-service:
    image: ghcr.io/is-csms-y26/lab-5-tools:master
    platform: linux/amd64
    container_name: order-processing-service
    networks:
      - order-processing-service-network
    depends_on:
      - postgres
      - kafka
    ports:
      - '8080:8080'
    environment:
      Infrastructure__Persistence__Postgres__Host: postgres
      Infrastructure__Persistence__Postgres__Database: postgres
      Infrastructure__Persistence__Postgres__Username: postgres
      Infrastructure__Persistence__Postgres__Password: postgres
      Presentation__Kafka__Host: kafka:9094
  
  zookeeper:
    image: wurstmeister/zookeeper:latest
    restart: unless-stopped
    environment:
      - ALLOW_ANONYMOUS_LOGIN=yes
    networks:
      - order-processing-service-network
  
  kafka:
    image: wurstmeister/kafka:latest
    restart: unless-stopped
    environment:
      KAFKA_LOG_DIRS: /kafka-data
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_INTER_BROKER_LISTENER_NAME: INTERNAL
      KAFKA_LISTENERS: EXTERNAL://:9092,INTERNAL://:9094
      KAFKA_ADVERTISED_LISTENERS: EXTERNAL://127.0.0.1:8001,INTERNAL://kafka:9094
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: EXTERNAL:PLAINTEXT,INTERNAL:PLAINTEXT
      ALLOW_PLAINTEXT_LISTENER: yes
      KAFKA_CREATE_TOPICS: >
        order_creation:1:1,
        order_processing:1:1,
    depends_on:
      - zookeeper
    networks:
      - order-processing-service-network
    volumes:
      - order-processing-service-kafka-data:/kafka-data
    ports:
      - '8001:9092'

  kafka-ui:
    image: provectuslabs/kafka-ui:latest
    build:
      context: .
    restart: unless-stopped
    depends_on:
      - kafka
    networks:
      - order-processing-service-network
    ports:
      - "8003:8080"
    volumes:
      - ./lab-4/Lab4.Kafka/protos:/schemas
    environment:
      kafka.clusters.0.name: kafka
      kafka.clusters.0.bootstrapServers: kafka:9094
      kafka.clusters.0.defaultKeySerde: ProtobufFile
      kafka.clusters.0.defaultValueSerde: ProtobufFile
      
      kafka.clusters.0.serde.0.name: ProtobufFile
      kafka.clusters.0.serde.0.properties.protobufFilesDir: /schemas/
      
      kafka.clusters.0.serde.0.properties.protobufMessageNameForKeyByTopic.order_creation: orders.OrderCreationKey
      kafka.clusters.0.serde.0.properties.protobufMessageNameForKeyByTopic.order_processing: orders.OrderProcessingKey
      
      kafka.clusters.0.serde.0.properties.protobufMessageNameByTopic.order_creation: orders.OrderCreationValue
      kafka.clusters.0.serde.0.properties.protobufMessageNameByTopic.order_processing: orders.OrderProcessingValue

networks:
  order-processing-service-network:
    driver: bridge

volumes:
  order-processing-service-kafka-data:
```
