# QualityWaterAlert Specification

## 1. Project Mission

To provide French citizens with a clear, accessible, and proactive way to monitor the quality of their drinking water. The application will present official government data in a user-friendly format, compare it against regulatory standards, and alert users to potential quality issues in their commune.

## 2. User Stories

### Epic: Water Quality Monitoring

- **As a user, I want to search for water quality data by commune name or postal code** so that I can easily find information relevant to my location.
- **As a user, I want to view the latest water quality analysis results for my selected commune** presented in a clean and understandable format (e.g., tables, charts).
- **As a user, I want to see a clear comparison between the measured water quality parameters and the official quality limits** so I can quickly assess if the water is compliant with standards.
- **As a user, I want to see visual indicators (e.g., color-coding, icons) to quickly understand if a parameter is within or outside the acceptable range.**

### Epic: Alert System

- **As a user, I want to subscribe to email alerts for a specific commune by clicking a bell icon** so that I am proactively notified when new data indicates a drop in water quality below the required standards.
- **As a user, I want to provide my email address to receive alerts.**
- **As a user, I want to receive a confirmation email after subscribing to alerts.**
- **As a user, I want to be able to unsubscribe from alerts easily** through a link in the notification email.

## 3. Functional Requirements

### 3.1. Data Display
- The application must fetch data from the specified `data.gouv.fr` API.
- Data must be displayed in a structured and clean manner, using a combination of tables and charts.
- For each water quality parameter, the application will display:
    - Parameter Name (e.g., "Nitrates")
    - Measured Value
    - Regulatory Limit/Required Value
    - Unit of Measurement
    - Date of Measurement
- A visual cue (e.g., green for compliant, red for non-compliant) will indicate the status of each parameter.

### 3.2. Alerting
- A bell icon will be displayed next to the commune's data.
- Clicking the bell icon will prompt the user to enter their email address to subscribe to alerts for that commune.
- The system will send an email notification to the user if a new analysis for the subscribed commune shows one or more parameters exceeding the regulatory limits.
- The email will contain a summary of the non-compliant parameters and a link back to the application.
- The system must include a mechanism for users to unsubscribe from alerts.

## 4. Non-Functional Requirements

- **UI/UX**: The interface must be clean, modern, and straight-to-the-data. It should be responsive and work seamlessly on both desktop and mobile devices.
- **Performance**: The application should load data quickly and provide a smooth user experience. API calls should be optimized.
- **Security**: User email addresses must be stored securely and handled in accordance with privacy best practices.
- **Reliability**: The application should be available and functioning correctly 24/7.

## 5. Out of Scope (for Version 1.0)

- Mobile application (planned for future releases)
- User accounts and login systems (subscriptions are managed directly via email).
- Alerts via channels other than email (e.g., SMS, push notifications).
- Advanced historical data analysis and trend visualization.
- Support for multiple languages (the initial version will be in French).
