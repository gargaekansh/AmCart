#!/bin/bash


# /mnt/c/D/Private/Nagarro/NagarroTraining/Nagp24/ECom/AmCart/DeploymentScripts

   # Make the script executable (only once):
     # chmod +x docker_start_services.sh
	 
 # Run a specific service:
 
	# ./docker_start_services.sh catalogdb1  # Start only MongoDB
	# ./docker_start_services.sh identitydb  # Start SQL Server
	# ./docker_start_services.sh elasticsearch  # Start Elasticsearch
	# ./docker_start_services.sh kibana  # Start Kibana
	# ./docker_start_services.sh amcart.catalog.api  # Start Catalog API
	# ./docker_start_services.sh amcart.identity.api  # Start Identity API


# Fix 1: Convert Line Endings from CRLF to LF
# If your script was edited in Windows (e.g., Notepad, VS Code), it likely has CRLF instead of LF line endings. Convert it using:

# sh
# Copy
# Edit
# sed -i 's/\r$//' docker_start_services.sh

# or bash docker_start_services.sh catalogdb1


  # Run all services at once:  
    # ./start_services.sh all

set -e  # Exit on error

log_info() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') 🔹 INFO: $1"
}

log_success() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') ✅ SUCCESS: $1"
}

log_error() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') ❌ ERROR: $1"
  exit 1
}

# Function to check if a container exists
check_container_exists() {
  docker ps -a --format "{{.Names}}" | grep -q "^$1$"
}

# Function to start a container if it does not exist
start_service() {
  local name=$1
  local command=$2

  if check_container_exists "$name"; then
    log_info "Container $name already exists."

    if docker ps --format "{{.Names}}" | grep -q "^$name$"; then
      log_info "$name is already running."
    else
      log_info "Starting existing container: $name"
      docker start "$name" || log_error "Failed to start $name"
    fi
  else
    log_info "Creating and starting $name..."
    eval "$command"
  fi
}

# Check for an argument (specific service)
SERVICE=$1

# # Create network if it does not exist
# if docker network ls | grep -q amcart_network; then
  # log_info "Network 'amcart_network' already exists."
# else
  # log_info "Creating Docker network: amcart_network..."
  # docker network create amcart_network || log_error "Failed to create network"
  # log_success "Network 'amcart_network' created successfully!"
# fi

# Create network if it does not exist
if ! docker network inspect amcart_network >/dev/null 2>&1; then
  log_info "Creating Docker network: amcart_network..."
  docker network create amcart_network || log_error "Failed to create network"
  log_success "Network 'amcart_network' created successfully!"
else
  log_info "Network 'amcart_network' already exists."
fi



# Create volumes if they don’t exist
for volume in mongo_data identity_data elasticsearch_data; do
  if docker volume ls | grep -q $volume; then
    log_info "Volume '$volume' already exists."
  else
    log_info "Creating volume: $volume..."
    docker volume create $volume || log_error "Failed to create volume $volume"
    log_success "Volume '$volume' created successfully!"
  fi
done

# Services List
case $SERVICE in
  catalogdb1)
    start_service "catalogdb1" "docker run -d --name catalogdb1 --restart always --network amcart_network -p 27017:27017 -v mongo_data:/data/db mongo"
    ;;
  identitydb)
    start_service "identitydb" "docker run -d --name identitydb --restart always --network amcart_network -e SA_PASSWORD='Pass@word123' -e ACCEPT_EULA='Y' -p 1444:1433 -v identity_data:/var/opt/mssql mcr.microsoft.com/mssql/server:2017-latest"
    ;;
  elasticsearch)
    start_service "elasticsearch" "docker run -d --name elasticsearch --restart always --network amcart_network -p 9200:9200 -p 9300:9300 -e 'discovery.type=single-node' -e 'ELASTIC_PASSWORD=changeme' -e 'xpack.watcher.enabled=false' -e 'xpack.security.enabled=true' -e 'ES_JAVA_OPTS=-Xms512m -Xmx512m' -v elasticsearch_data:/usr/share/elasticsearch/data docker.elastic.co/elasticsearch/elasticsearch:8.5.0"
    ;;
  kibana)
    start_service "kibana" "docker run -d --name kibana --restart always --network amcart_network -p 5601:5601 kibana:8.5.0"
    ;;
  amcart.catalog.api)
    start_service "amcart.catalog.api" "docker run -d --name amcart.catalog.api --restart always --network amcart_network -p 8000:8080 -e 'ASPNETCORE_ENVIRONMENT=Development' -e 'DatabaseSettings:ConnectionString=mongodb://catalogdb1:27017' amcart.catalog.api:1.0.1"
    ;;
  amcart.identity.api)
    start_service "amcart.identity.api" "docker run -d --name amcart.identity.api --restart always --network amcart_network -p 8003:8080 -p 8004:443 -e 'ASPNETCORE_ENVIRONMENT=Development' -e 'ConnectionStrings:IdentityDb=Server=identitydb;Database=IdentityDb0;User Id=sa;Password=Pass@word123;TrustServerCertificate=True;' amcart.identity.api"
    ;;
  all)
    log_info "Starting all services..."
    $0 catalogdb1
    $0 identitydb
    $0 elasticsearch
    $0 kibana
    $0 amcart.catalog.api
    $0 amcart.identity.api
    ;;
  *)
    echo "Usage: $0 {catalogdb1 | identitydb | elasticsearch | kibana | amcart.catalog.api | amcart.identity.api | all}"
    exit 1
    ;;
esac

log_success "$SERVICE started successfully!"
