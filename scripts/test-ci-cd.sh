#!/bin/bash

# Local Development and Testing Script for Perim App
# This script helps developers test the CI/CD pipeline locally

set -e

echo "🚀 Perim App - Local CI/CD Testing Script"
echo "========================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
print_status() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Check prerequisites
check_prerequisites() {
    echo "🔍 Checking prerequisites..."
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        exit 1
    fi
    print_status "Docker is available"
    
    if ! command -v dotnet &> /dev/null; then
        print_warning "dotnet CLI not found - Docker will be used for builds"
    else
        DOTNET_VERSION=$(dotnet --version)
        print_status "dotnet CLI available (version: $DOTNET_VERSION)"
    fi
}

# Validate workflow files
validate_workflows() {
    echo "📋 Validating GitHub Actions workflows..."

    PYTHON_CMD=""

    if command -v python3 &> /dev/null; then
        PYTHON_CMD="python3"
    elif command -v python &> /dev/null; then
        PYTHON_CMD="python"
    elif command -v py &> /dev/null; then
        PYTHON_CMD="py"
    fi
    
    if [ -n "$PYTHON_CMD" ]; then
        $PYTHON_CMD -c "
import yaml
import sys

workflows = [
    '.github/workflows/ci.yml',
    '.github/workflows/cd.yml', 
    '.github/workflows/security.yml',
    '.github/workflows/manual-validation.yml'
]

for workflow in workflows:
    try:
        with open(workflow, 'r', encoding='utf-8') as f:
            yaml.safe_load(f)
        print(f'✅ {workflow} - Valid YAML syntax')
    except Exception as e:
        print(f'❌ {workflow} - Invalid YAML: {e}')
        sys.exit(1)
"
        print_status "All workflow files have valid YAML syntax"
    else
        print_warning "Python3 not available - skipping YAML validation"
    fi
}

# Test Docker build
test_docker_build() {
    echo "🐳 Testing Docker build..."
    
    if docker build -t perimapp:local-test . > /dev/null 2>&1; then
        print_status "Docker build successful"
        
        # Clean up test image
        docker rmi perimapp:local-test > /dev/null 2>&1 || true
    else
        print_error "Docker build failed"
        echo "Run 'docker build -t perimapp:local-test .' to see detailed error output"
        return 1
    fi
}

# Test .NET build (if available)
test_dotnet_build() {
    if command -v dotnet &> /dev/null; then
        echo "🔨 Testing .NET build..."
        
        cd perimapp
        if dotnet restore > /dev/null 2>&1; then
            print_status ".NET restore successful"
        else
            print_warning ".NET restore failed (may require .NET 9.0)"
        fi
        cd ..
    fi
}

# Main execution
main() {
    echo "Starting local CI/CD validation..."
    echo
    
    check_prerequisites
    echo
    
    validate_workflows
    echo
    
    test_docker_build
    echo
    
    test_dotnet_build
    echo
    
    print_status "Local CI/CD testing completed!"
    echo
    echo "📖 Next steps:"
    echo "  - Review ci-cd.md for detailed documentation"
    echo "  - Push changes to trigger actual CI/CD pipeline"
    echo "  - Monitor GitHub Actions for workflow execution"
}

# Parse command line arguments
case "${1:-}" in
    --docker-only)
        echo "🐳 Running Docker-only tests..."
        check_prerequisites
        test_docker_build
        ;;
    --validate-only)
        echo "📋 Running validation-only tests..."
        validate_workflows
        ;;
    --help|-h)
        echo "Usage: $0 [--docker-only|--validate-only|--help]"
        echo "  --docker-only    Test Docker build only"
        echo "  --validate-only  Validate workflow files only"
        echo "  --help          Show this help message"
        ;;
    *)
        main
        ;;
esac