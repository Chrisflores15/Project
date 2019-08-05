USE runtime
;

DROP TABLE IF EXISTS `Account`
;

CREATE TABLE runtime.Account(
	-- AccountId INT AUTO_INCREMENT NOT NULL,
    EmployeeId VARCHAR(15) NOT NULL UNIQUE,
    AcctPassword VARCHAR(255) NOT NULL,
    FirstName VARCHAR(60) NOT NULL,
    LastName VARCHAR(60) NOT NULL,
    Email VARCHAR(60) NOT NULL,
    AcctCreateDt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- AccountRequestId INT,
    AdminFlag CHAR(1), -- Used to tell if account is admin
    DisableFlag CHAR(1), -- Used to tell if account is disabled by admin
    DisableCount INT NOT NULL DEFAULT 0, -- Used to tell admin how many times account has been disabled.
    CONSTRAINT Pk_Account PRIMARY KEY (EmployeeId),
    -- CONSTRAINT Fk_Account FOREIGN KEY Fk_Account_AccountRequest (AccountRequestId) REFERENCES AccountRequest(AccountRequestId)
		-- ON DELETE SET NULL,
    CONSTRAINT Ck_Account_AdminFlag CHECK (AdminFlag IN ('X')),
    CONSTRAINT Ck_Account_DisableFlag CHECK (DisableFlag IN ('X')),
    CONSTRAINT Ck_Account_DisableCount CHECK (DisableCount >=0)
)
;

INSERT INTO runtime.Account (EmployeeId, AcctPassword, FirstName, LastName, Email)
	SELECT EmployeeId, AcctPassword, FirstName, LastName, Email FROM runtime.AccountRequest WHERE AccountApproval = 'A'
;

UPDATE runtime.Account
	SET AdminFlag = 'X' WHERE EmployeeId = '1003'
;

SELECT * FROM runtime.Account
;