#!/bin/bash

set -e

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

SERVICE=$1

stop_service() {
  if docker ps -q -f name=$1 | grep -q .; then
    docker stop $1 && docker rm $1 || log_error "Failed to stop/remove $1"
    log_success "Stopped and removed $1"
  else
    log_info "$1 is not running."
  fi
}

case $SERVICE in
  catalogdb1|identitydb|elasticsearch|kibana|amcart.catalog.api|amcart.identity.api)
    stop_service $SERVICE
    ;;
  all)
    for service in catalogdb1 identitydb elasticsearch kibana amcart.catalog.api amcart.identity.api; do
      stop_service $service
    done
    ;;
  *)
    echo "Usage: $0 {catalogdb1 | identitydb | elasticsearch | kibana | amcart.catalog.api | amcart.identity.api | all}"
    exit 1
    ;;
esac

log_success "$SERVICE stopped successfully!"
