# Dataverse Attendance Management Plugins

This folder contains C# plugin examples for enforcing business rules and automating employee, department, and attendance operations in Microsoft Dataverse. The plugins use the `oma_` table and column logical names defined in `schema`.

## What’s Included

### Employee plugins

| Class | Behavior | Registration described in source |
|---|---|---|
| `newEmp` | On employee create or update, allows names containing only English letters and spaces. | Employee: Create and Update, PreOperation, synchronous |
| `experienceYearsAutoCalculation` | Calculates experience from the hire date to the current date and rounds to two decimal places. | Employee: Create, PreOperation, synchronous |
| `shiftCapacityValidator` | Blocks an employee shift assignment when the selected shift already has two or more employees. | Employee: Create, PreValidation, synchronous |
| `EmployeeCount` | Updates the related department’s employee count on employee create, department change, and delete. | Employee: Create, Update, and Delete, PostOperation, synchronous |

`EmployeeCount` requires entity images for the department lookup:

- Update: a pre-image named `Update Employee Count PreImage`, containing `oma_department`
- Delete: a pre-image named `Delete Employee Count PreImage`, containing `oma_department`

### Department plugin

| Class | Behavior | Registration described in source |
|---|---|---|
| `preventDepartmentDeletion` | Prevents deleting a department while employee records are associated with it. | Department: Delete, PreValidation, synchronous |

### Attendance plugins

| Class | Behavior | Registration described in source |
|---|---|---|
| `attendanceDuplicateCheck` | Blocks creating another attendance record for the same employee on the same calendar date, based on check-in time. | Attendance: Create, PreValidation, synchronous |
| `attendanceHours` | Requires checkout to be later than check-in when both values are present, then calculates work hours rounded to two decimal places. | Attendance: Create, PreOperation, synchronous |
| `autoAssignAttendance` | Copies the employee’s department and shift lookups onto a newly created attendance record. | Attendance: Create, PostOperation, synchronous |

`attendanceDuplicateCheck` sets the `AttendanceValid` shared variable, which `autoAssignAttendance` reads when it updates an attendance record. When registering the attendance steps, verify that the steps execute in a compatible pipeline and that the shared variable is available to the post-operation step.

## Dataverse Schema

The constants in `schema` describe the logical names expected by the plugins:

- **Employee:** `oma_employee` — includes employee name, department, email, employee code, experience years, hire date, and shift.
- **Department:** `oma_department` — includes department name, contact number, code, employee count, location, and manager.
- **Attendance:** `oma_attendance` — includes employee, department, shift, check-in/check-out times, work hours, status, and related attendance fields.

Create these custom tables, columns, and relationships in the target Dataverse environment, or update the constants and plugin logic to match its schema.

## Build and Deployment

This directory contains C# source files only; it does not include a `.csproj`, solution file, tests, or a deployment package. The classes are not runnable as a standalone application.

To use them:

1. Add the source files to a Dataverse-compatible plugin class library that references the Dataverse SDK.
2. Build the assembly using a framework and SDK version supported by your Dataverse plugin environment.
3. Register the assembly and plugin steps with the Plugin Registration Tool or your established deployment process.
4. Configure each step’s message, table, stage, execution mode, filtering attributes (where appropriate), and entity images to match the requirements above and the registration comments in the source.
5. Test the plugins in a non-production environment, including invalid data, missing lookups, duplicate attendance, department reassignment, and employee-count changes.

## Repository Layout

```text
dataverse_plugins/
├── schema/
│   ├── attendance.cs
│   ├── department.cs
│   └── employee.cs
└── tables/
    ├── attendance/
    ├── department/
    └── emp/
```
