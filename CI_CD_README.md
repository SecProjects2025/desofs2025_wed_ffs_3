# AMAP CI/CD Pipeline Documentation

## Overview
This document describes the Continuous Integration and Continuous Deployment (CI/CD) pipeline implemented for the AMAP project using GitHub Actions.

## Pipeline Architecture

### Trigger Conditions
- Push to main branch
- Pull requests to main branch

### Pipeline Stages

#### 1. Build Stage
- **Purpose**: Compile and verify the application
- **Actions**:
  - Restore NuGet packages
  - Build solution (AMAP/AMAP.sln)
  - Run unit tests
- **Tools**: .NET 8.0

#### 2. Code Quality Analysis (SonarCloud)
- **Purpose**: Ensure code quality and maintainability
- **Features**:
  - Static code analysis
  - Code coverage reporting
  - Code smell detection
  - Bug detection
- **Configuration**:
  - Project Key: `diogoc0307_desofs`
  - Organization: `diogoc0307`
- **Limitations** (Free Tier):
  - Analysis limited to main branch
  - Limited lines of code
  - Basic code quality metrics

#### 3. Security Analysis (SCA)
- **Purpose**: Identify security vulnerabilities
- **Tool**: OWASP Dependency Check
- **Output**: Security analysis results available in GitHub Actions artifacts
- **Scope**: All project dependencies

#### 4. Deployment Stage
- **Purpose**: Deploy to staging environment
- **Trigger**: Only on main branch
- **Status**: Configuration pending

## Configuration

### Required Secrets
1. `SONAR_TOKEN`
   - Purpose: SonarCloud authentication
   - Location: GitHub repository secrets
   - How to get: https://sonarcloud.io/account/security

### Environment Variables
```yaml
DOTNET_VERSION: '8.0.x'
```

## How to Use

### Running the Pipeline
1. **Automatic Trigger**
   - Push to main branch
   - Create pull request

2. **Manual Trigger**
   - Go to Actions tab in GitHub
   - Select workflow
   - Click "Run workflow"

### Viewing Results
1. **Pipeline Status**
   - GitHub Actions tab
   - Check individual job status

2. **Code Quality**
   - SonarCloud dashboard
   - Project metrics
   - Code coverage

3. **Security Analysis Results**
   - Available in GitHub Actions artifacts
   - Download from the Actions tab after pipeline completion

## Troubleshooting

### Common Issues

1. **Pipeline Fails to Start**
   - Check GitHub Actions permissions
   - Verify repository secrets
   - Ensure workflow file is in correct location

2. **SonarCloud Analysis Fails**
   - Verify SONAR_TOKEN is set
   - Check project key and organization
   - Ensure main branch is properly configured

3. **Build Failures**
   - Check .NET version compatibility
   - Verify solution path
   - Check for compilation errors

## Best Practices

1. **Code Quality**
   - Run local analysis before pushing
   - Address SonarCloud issues promptly
   - Maintain test coverage

2. **Security**
   - Regular dependency updates
   - Review security analysis results
   - Address vulnerabilities promptly

3. **Development Workflow**
   - Create feature branches
   - Use pull requests
   - Review pipeline results

## Future Improvements

1. **Planned Enhancements**
   - Production deployment configuration
   - Automated versioning
   - Performance testing integration

2. **Potential Additions**
   - Docker containerization
   - Automated database migrations
   - Environment-specific configurations

## Support

For pipeline-related issues:
1. Check troubleshooting guide
2. Review GitHub Actions documentation
3. Contact project maintainers

## References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [SonarCloud Documentation](https://docs.sonarcloud.io)
- [OWASP Dependency Check](https://owasp.org/www-project-dependency-check/) 