USE runtime
;
DROP TABLE IF EXISTS `AccountRequest`
;
CREATE TABLE runtime.AccountRequest (
	-- AccountRequestId INT NOT NULL AUTO_INCREMENT,
    EmployeeId VARCHAR(15) NOT NULL,
    AcctPassword VARCHAR(255) NOT NULL,
    FirstName VARCHAR(60) NOT NULL,
    LastName VARCHAR(60) NOT NULL,
    Email VARCHAR(60) NOT NULL,
    RequestDt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AccountApproval CHAR(1) DEFAULT NULL,
    CONSTRAINT Pk_AccountRequest PRIMARY KEY (EmployeeId),
    CONSTRAINT Ck_AccountRequest_AccountApproval CHECK (AccountApproval IN ('A', 'D'))
)
;

INSERT INTO runtime.AccountRequest (EmployeeId, AcctPassword, FirstName, LastName, Email, AccountApproval)
	VALUES
		('1000', 'Pass1000', 'Chris', 'Weston', 'Chris.Weston@Runtime.com', 'A'),
        ('1001', 'Pass1001', 'Valerie', 'Tran', 'Valerie.Tran@Runtime.com', 'A'),
        ('1002', 'Pass1002', 'Amit', 'Runwal', 'Amit.Runwal@Runtime.com', 'D'),
        ('1003', 'Pass1003', 'Jon', 'Sherman', 'Jon.Sherman@Runtime.com', 'A'),
        ('1004', 'Pass1004', 'Robert', 'Kebert', 'Robert.Kebert@Runtime.com', 'D'),
        ('1005', 'Pass1005', 'Lalit', 'Wadwah', 'Lalit.Wadwah@Runtime.com', NULL),
        ('1006', 'Pass1006', 'Khoi', 'Tran', 'Khoi.Tran@Runtime.com', NULL),
        ('1007', 'Pass1007', 'Aron', 'Singh', 'Aron.Singh@Runtime.com', NULL),
        ('1008', 'Pass1007', 'Jack', 'McCarrick', 'Aron.Singh@Runtime.com', NULL),
        ('1009', 'Pass1007', 'Lauren', 'Nilsen', 'Aron.Singh@Runtime.com', NULL)
;

SELECT * FROM runtime.AccountRequest;