-- Read-only audit of the isolated BACPAC import. No application data is changed.
SET NOCOUNT ON;
IF DB_NAME() <> N'RopeParison_Review_20260917'
    THROW 50001, 'Run this audit only against the isolated review database.', 1;

SELECT 'Counts' AS Section, 'Ropes' AS Entity, COUNT(*) AS Rows FROM dbo.Ropes
UNION ALL SELECT 'Counts', 'Brands', COUNT(*) FROM dbo.Brands
UNION ALL SELECT 'Counts', 'CalculatedParameters', COUNT(*) FROM dbo.RopeCalculatedParameterSets
UNION ALL SELECT 'Counts', 'EditSuggestions', COUNT(*) FROM dbo.RopeEditSuggestions
UNION ALL SELECT 'Counts', 'Migrations', COUNT(*) FROM dbo.__EFMigrationsHistory;
SELECT 'Categories' AS Section, Category, Verified, COUNT(*) AS Ropes FROM dbo.Ropes GROUP BY Category, Verified ORDER BY Category, Verified;
SELECT 'Migrations' AS Section, MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;

SELECT 'DuplicateNames' AS Section, LTRIM(RTRIM(Name)) AS NormalizedName, COUNT(*) AS Matches,
    STRING_AGG(CONVERT(nvarchar(max), RopeId), ',') AS RopeIds
FROM dbo.Ropes GROUP BY LTRIM(RTRIM(Name)) HAVING COUNT(*) > 1;
SELECT 'DuplicateBrandNames' AS Section, LTRIM(RTRIM(Name)) AS NormalizedName, COUNT(*) AS Matches
FROM dbo.Brands GROUP BY LTRIM(RTRIM(Name)) HAVING COUNT(*) > 1;
SELECT 'InvalidBasics' AS Section, RopeId, Name, Category, Diameter, MassPerUnitLength, SheathPercentage, SheathSlippage
FROM dbo.Ropes WHERE Name IS NULL OR LEN(LTRIM(RTRIM(Name))) = 0 OR LEN(Name) > 150
    OR Category NOT IN (0,1,2,3,4) OR Diameter <= 0 OR MassPerUnitLength <= 0
    OR SheathPercentage < 0 OR SheathPercentage > 100 OR SheathSlippage < 0;

SELECT 'Orphans' AS Section,
    (SELECT COUNT(*) FROM dbo.Ropes r LEFT JOIN dbo.Brands b ON b.BrandId=r.BrandId WHERE b.BrandId IS NULL) AS MissingBrand,
    (SELECT COUNT(*) FROM dbo.Ropes r LEFT JOIN dbo.RopeCalculatedParameterSets c ON c.RopeId=r.RopeId WHERE c.RopeId IS NULL) AS MissingCalculation,
    (SELECT COUNT(*) FROM dbo.RopeCalculatedParameterSets c LEFT JOIN dbo.Ropes r ON r.RopeId=c.RopeId WHERE r.RopeId IS NULL) AS OrphanCalculation,
    (SELECT COUNT(*) FROM dbo.RopeEditSuggestions s LEFT JOIN dbo.Ropes r ON r.RopeId=s.RopeId WHERE r.RopeId IS NULL) AS OrphanSuggestion;

SELECT 'MeasurementIssues' AS Section, r.RopeId, r.Name, r.Category, r.Verified, f.FieldName, f.Value,
    CASE WHEN f.Applicable=0 THEN 'InapplicableValue' WHEN f.Value IS NULL THEN 'MissingApplicableValue' ELSE 'NonpositiveValue' END AS Issue
FROM dbo.Ropes r CROSS APPLY (VALUES
    ('ImpactForce55kgOneStrand',r.ImpactForce55kgOneStrand,CASE WHEN Category IN (1,3,4) THEN 1 ELSE 0 END),
    ('ImpactForce80kgOneStrand',r.ImpactForce80kgOneStrand,CASE WHEN Category IN (0,4) THEN 1 ELSE 0 END),
    ('ImpactForce80kgTwoStrand',r.ImpactForce80kgTwoStrand,CASE WHEN Category IN (2,3,4) THEN 1 ELSE 0 END),
    ('StaticElongation80kgOneStrand',r.StaticElongation80kgOneStrand,CASE WHEN Category IN (0,1,3,4) THEN 1 ELSE 0 END),
    ('StaticElongation80kgTwoStrand',r.StaticElongation80kgTwoStrand,CASE WHEN Category IN (2,3,4) THEN 1 ELSE 0 END),
    ('DynamicElongation55kgOneStrand',r.DynamicElongation55kgOneStrand,CASE WHEN Category IN (1,3,4) THEN 1 ELSE 0 END),
    ('DynamicElongation80kgOneStrand',r.DynamicElongation80kgOneStrand,CASE WHEN Category IN (0,4) THEN 1 ELSE 0 END),
    ('DynamicElongation80kgTwoStrand',r.DynamicElongation80kgTwoStrand,CASE WHEN Category IN (2,3,4) THEN 1 ELSE 0 END),
    ('DropsBeforeBreak55kgOneStrand',CONVERT(float,r.DropsBeforeBreak55kgOneStrand),CASE WHEN Category IN (1,3,4) THEN 1 ELSE 0 END),
    ('DropsBeforeBreak80kgOneStrand',CONVERT(float,r.DropsBeforeBreak80kgOneStrand),CASE WHEN Category IN (0,4) THEN 1 ELSE 0 END),
    ('DropsBeforeBreak80kgTwoStrand',CONVERT(float,r.DropsBeforeBreak80kgTwoStrand),CASE WHEN Category IN (2,3,4) THEN 1 ELSE 0 END)
) f(FieldName,Value,Applicable)
WHERE (f.Applicable=0 AND f.Value IS NOT NULL) OR (f.Applicable=1 AND (f.Value IS NULL OR f.Value <= 0))
ORDER BY r.RopeId, f.FieldName;

;WITH Expected AS (
 SELECT r.*, PI()*POWER(r.Diameter/2.0,2) AS ExpectedArea,
    SQRT(CASE WHEN r.SheathPercentage BETWEEN 0 AND 100 THEN 1-r.SheathPercentage/100.0 END)*r.Diameter AS ExpectedCoreDiameter
 FROM dbo.Ropes r
), Differences AS (
 SELECT r.RopeId,r.Name,r.Verified,r.Diameter,r.SheathPercentage,
        c.SheathThickness,(r.Diameter-r.ExpectedCoreDiameter)/2.0 AS CorrectedThickness,
        f.FieldName,f.Stored,f.Expected
 FROM Expected r JOIN dbo.RopeCalculatedParameterSets c ON r.RopeId=c.RopeId
 CROSS APPLY (VALUES
    ('Area',c.Area,r.ExpectedArea),
    ('Density',c.Density,r.MassPerUnitLength/NULLIF(r.ExpectedArea*1000,0)),
    ('SheathArea',c.SheathArea,r.ExpectedArea*r.SheathPercentage/100.0),
    ('CoreArea',c.CoreArea,r.ExpectedArea*(1-r.SheathPercentage/100.0)),
    ('CoreDiameter',c.CoreDiameter,r.ExpectedCoreDiameter),
    ('SheathThickness_CurrentCode',c.SheathThickness,r.Diameter-r.ExpectedCoreDiameter),
    ('SheathThickness_Corrected',c.SheathThickness,(r.Diameter-r.ExpectedCoreDiameter)/2.0)
 ) f(FieldName,Stored,Expected)
)
SELECT 'CalculationDifferences' AS Section, FieldName,
    COUNT(*) AS ComparedRows,
    SUM(CASE WHEN (Stored IS NULL AND Expected IS NOT NULL) OR (Stored IS NOT NULL AND Expected IS NULL)
      OR ABS(Stored-Expected) > 1e-8*(1+ABS(Expected)) THEN 1 ELSE 0 END) AS Mismatches
FROM Differences GROUP BY FieldName ORDER BY FieldName;

SELECT TOP (5) 'ThicknessExamples' AS Section,r.RopeId,r.Name,r.Diameter,r.SheathPercentage,c.CoreDiameter,c.SheathThickness,
    (r.Diameter-c.CoreDiameter)/2 AS CorrectedThickness
FROM dbo.Ropes r JOIN dbo.RopeCalculatedParameterSets c ON c.RopeId=r.RopeId
WHERE c.SheathThickness IS NOT NULL AND ABS(c.SheathThickness-(r.Diameter-c.CoreDiameter)/2)>1e-8 ORDER BY r.RopeId;

SELECT 'Suggestions' AS Section,r.Name AS CurrentRopeName,r.Category AS CurrentCategory,r.Verified,s.*
FROM dbo.RopeEditSuggestions s LEFT JOIN dbo.Ropes r ON r.RopeId=s.RopeId ORDER BY s.RopeEditSuggestionId;

SELECT 'NullsAndBounds' AS Section, SUM(CASE WHEN SheathPercentage IS NULL THEN 1 ELSE 0 END) AS MissingSheath,
    SUM(CASE WHEN SheathSlippage IS NULL THEN 1 ELSE 0 END) AS MissingSlippage,
    MIN(Diameter) AS MinDiameter,MAX(Diameter) AS MaxDiameter,
    MIN(MassPerUnitLength) AS MinMass,MAX(MassPerUnitLength) AS MaxMass,
    MIN(SheathPercentage) AS MinSheath,MAX(SheathPercentage) AS MaxSheath
FROM dbo.Ropes;

SELECT 'Indexes' AS Section,OBJECT_NAME(i.object_id) AS TableName,i.name,i.is_unique,i.is_primary_key,
    STRING_AGG(CONVERT(nvarchar(max),c.name),',') WITHIN GROUP (ORDER BY ic.key_ordinal) AS KeyColumns
FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE OBJECT_SCHEMA_NAME(i.object_id)='dbo' AND ic.key_ordinal > 0
GROUP BY i.object_id,i.name,i.is_unique,i.is_primary_key ORDER BY TableName,i.name;
SELECT 'ConcurrencyColumns' AS Section,OBJECT_NAME(object_id) AS TableName,name FROM sys.columns WHERE system_type_id=189;
SELECT 'ForeignKeys' AS Section,name,is_disabled,is_not_trusted FROM sys.foreign_keys;
SELECT 'PendingTargetAndUnverified' AS Section,RopeId,Name,Diameter,Category,Verified
FROM dbo.Ropes WHERE RopeId=1060 OR Verified=0;
SELECT 'ThicknessByVerification' AS Section,r.Verified,COUNT(*) AS ThicknessRows
FROM dbo.Ropes r JOIN dbo.RopeCalculatedParameterSets c ON c.RopeId=r.RopeId
WHERE c.SheathThickness IS NOT NULL GROUP BY r.Verified;
SELECT 'Collation' AS Section,CONVERT(nvarchar(128),DATABASEPROPERTYEX(DB_NAME(),'Collation')) AS Collation;
