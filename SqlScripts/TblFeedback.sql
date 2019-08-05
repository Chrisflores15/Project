USE runtime
;

DROP TABLE IF EXISTS `Feedback`
;

CREATE TABLE runtime.Feedback(
	RecordId INT AUTO_INCREMENT NOT NULL,
    EmployeeId VARCHAR(15),
    FeedbackDate DATETIME NOT NULL DEFAULT NOW(),
    Department VARCHAR(60) NOT NULL,
    FeedbackText1 TEXT,
    FeedbackRating1 INT NOT NULL,
    FeedbackText2 TEXT,
    FeedbackRating2 INT NOT NULL,
    FeedbackText3 TEXT,
    FeedbackRating3 INT NOT NULL,
    FeedbackText4 TEXT,    
    FeedbackRating4 INT NOT NULL,
    CONSTRAINT Pk_Feedback PRIMARY KEY (RecordId),
    CONSTRAINT Fk_Feedback_Account FOREIGN KEY (EmployeeId) REFERENCES Account (EmployeeId)
		ON DELETE SET NULL,
	CONSTRAINT Ck_Feedback_FeedbackRating1 CHECK (FeedbackRating1 >= 0 AND FeedbackRating <= 5),
    CONSTRAINT Ck_Feedback_FeedbackRating2 CHECK (FeedbackRating2 >= 0 AND FeedbackRating <= 5),
    CONSTRAINT Ck_Feedback_FeedbackRating3 CHECK (FeedbackRating3 >= 0 AND FeedbackRating <= 5),
    CONSTRAINT Ck_Feedback_FeedbackRating4 CHECK (FeedbackRating4 >= 0 AND FeedbackRating <= 5),
	CONSTRAINT Ck_Feedback_Department CHECK (Department IN (
		'Sales', 'HR', 'IT', 'Marketing', 'Accounting/Finance', 'Customer Service', 'Operations', 'Distribution'))
)
;

INSERT INTO runtime.Feedback(EmployeeId, Department, FeedbackText1, FeedbackRating1, FeedbackText2, FeedbackRating2, FeedbackText3, FeedbackRating3, FeedbackText4, FeedbackRating4)
	VALUES
		('1000', 'Sales', 'This is example feedback for question 1.  This is negative feedback.', 2, 'This is example feedback for question 2.  This is average feedback.', 3, 'This is example feedback for question 3.  This is positive feedback.', 5, 'This is example feedback for question 4.  This is positive feedback.', 4),
		('1001', 'Human Resources', 'This is example feedback for question 1.  This is average feedback.', 3, 'This is example feedback for question 2.  This is negative feedback.', 1, 'This is example feedback for question 3.  This is negative feedback.', 2, 'This is example feedback for question 4.  This is average feedback.', 3),
        ('1003', 'IT', 'This is example feedback for question 1.  This is positive feedback.', 5, 'This is example feedback for question 2.  This is positive feedback.', 4, 'This is example feedback for question 3.  This is average feedback.', 3, 'This is example feedback for question 4.  This is negative feedback.', 1),
		('1000', 'Marketing', 'This is example feedback for question 1.  This is negative feedback.', 2, 'This is example feedback for question 2.  This is average feedback.', 3, 'This is example feedback for question 3.  This is positive feedback.', 4, 'This is example feedback for question 4.  This is average feedback.', 3),
		('1001', 'Accounting/Finance', 'This is example feedback for question 1.  This is negative feedback.', 1, 'This is example feedback for question 2.  This is average feedback.', 3, 'This is example feedback for question 3.  This is positive feedback.', 4, 'This is example feedback for question 4.  This is average feedback.', 3),
		('1003', 'Customer Service', 'This is example feedback for question 1.  This is negative feedback.', 1, 'This is example feedback for question 2.  This is average feedback.', 3, 'This is example feedback for question 3.  This is positive feedback.', 4, 'This is example feedback for question 4.  This is negative feedback.', 2),
		('1000', 'Operations', 'This is example feedback for question 1.  This is positive feedback.', 5, 'This is example feedback for question 2.  This is negative feedback.', 2, 'This is example feedback for question 3.  This is positive feedback.', 4, 'This is example feedback for question 4.  This is positive feedback.', 5),
		('1001', 'Distribution', 'This is example feedback for question 1.  This is positive feedback.', 4, 'This is example feedback for question 2.  This is average feedback.', 3, 'This is example feedback for question 3.  This is average feedback.', 3, 'This is example feedback for question 4.  This is negative feedback.', 1)
;

SELECT * FROM runtime.Feedback
;
	
/*
************************************************************
EXAMPLE QUERIES
*************************************************************
-- For reading all recent feedback with most recent at the top)
SELECT * FROM runtime.Feedback
	ORDER BY FeedbackDate DESC
;

-- For reading all feedback for a specific department...
	-- where @ denotes a parameter/variable value
SELECT * FROM runtime.Feedback
	WHERE Department = 'Sales'@Department
;

-- For writing feedback after user input...
	-- where @ denotes a parameter/variable value and {}indicates data type (would omit '{}' and contents from actual statement).  SEE BELOW FOR EXAMPLE WITHOUT DATA TYPES.
-- NOTE: FeedbackDate will be provided by default in table definition.
INSERT INTO runtime.Feedback(EmployeeId, Department, FeedbackText1, FeedbackRating1, FeedbackText2, FeedbackRating2, FeedbackText3, FeedbackRating3, FeedbackText4, FeedbackRating4)
	VALUES
		(@EmployeeId{string}, @Department{string}, @FeedbackText1{string}, @FeedbackRating1{int}, @FeedbackText2{string}, @FeedbackRating2{int}, @FeedbackText3{string}, @FeedbackRating3{int}, @FeedbackText4{string}, @FeedbackRating4{int})
;

-- For writing feedback after user input... (SAME AS ABOVE BUT WITHOUT DATA TYPES - EASIER TO COPY IF DATA TYPES ARE KNOWN)
	-- where @ denotes a parameter/variable value
-- NOTE: FeedbackDate will be provided by default in table definition.
INSERT INTO runtime.Feedback(EmployeeId, Department, FeedbackText1, FeedbackRating1, FeedbackText2, FeedbackRating2, FeedbackText3, FeedbackRating3, FeedbackText4, FeedbackRating4)
	VALUES
		(@EmployeeId, @Department, @FeedbackText1, @FeedbackRating1, @FeedbackText2, @FeedbackRating2, @FeedbackText3, @FeedbackRating3, @FeedbackText4, @FeedbackRating4)
;
*/



