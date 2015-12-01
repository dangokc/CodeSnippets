USE SMART_Amaris
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'PriorityDirector'))
BEGIN
	DROP TABLE dbo.PriorityDirector
END
ELSE
BEGIN
CREATE TABLE dbo.PriorityDirector
(	
	PriorityId bigint REFERENCES dbo.OpeningCountry_Priority(PriorityId),
	EmployeeId int REFERENCES dbo.Employee(EmployeeId),
	PRIMARY KEY (PriorityId, EmployeeId)
)
END