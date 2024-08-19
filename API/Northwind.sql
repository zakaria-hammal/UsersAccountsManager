/*
** Copyright Microsoft, Inc. 1994 - 2000
** All Rights Reserved.
*/

DROP TABLE IF EXISTS "Categories";
DROP TABLE IF EXISTS "Products";
DROP TABLE IF EXISTS "Suppliers";

CREATE TABLE "Categories" (
	"CategoryID" INTEGER PRIMARY KEY,
	"CategoryName" nvarchar (15) NOT NULL ,
	"Description" "ntext" NULL ,
	"Picture" "image" NULL
);
CREATE INDEX "CategoryName" ON "Categories"("CategoryName");

CREATE TABLE "Suppliers" (
	"SupplierID" INTEGER PRIMARY KEY,
	"CompanyName" nvarchar (40) NOT NULL ,
	"ContactName" nvarchar (30) NULL ,
	"ContactTitle" nvarchar (30) NULL ,
	"Address" nvarchar (60) NULL ,
	"City" nvarchar (15) NULL ,
	"Region" nvarchar (15) NULL ,
	"PostalCode" nvarchar (10) NULL ,
	"Country" nvarchar (15) NULL ,
	"Phone" nvarchar (24) NULL ,
	"Fax" nvarchar (24) NULL ,
	"HomePage" "ntext" NULL
);
CREATE INDEX "CompanyNameSuppliers" ON "Suppliers"("CompanyName");
CREATE INDEX "PostalCodeSuppliers" ON "Suppliers"("PostalCode");

CREATE TABLE "Products" (
	"ProductID" INTEGER PRIMARY KEY,
	"ProductName" nvarchar (40) NOT NULL ,
	"SupplierID" "int" NULL ,
	"CategoryID" "int" NULL ,
	"QuantityPerUnit" nvarchar (20) NULL ,
	"UnitPrice" "money" NULL CONSTRAINT "DF_Products_UnitPrice" DEFAULT (0),
	"UnitsInStock" "smallint" NULL CONSTRAINT "DF_Products_UnitsInStock" DEFAULT (0),
	"UnitsOnOrder" "smallint" NULL CONSTRAINT "DF_Products_UnitsOnOrder" DEFAULT (0),
	"ReorderLevel" "smallint" NULL CONSTRAINT "DF_Products_ReorderLevel" DEFAULT (0),
	"Discontinued" "bit" NOT NULL CONSTRAINT "DF_Products_Discontinued" DEFAULT (0),
	CONSTRAINT "FK_Products_Categories" FOREIGN KEY 
	(
		"CategoryID"
	) REFERENCES "Categories" (
		"CategoryID"
	),
	CONSTRAINT "FK_Products_Suppliers" FOREIGN KEY 
	(
		"SupplierID"
	) REFERENCES "Suppliers" (
		"SupplierID"
	),
	CONSTRAINT "CK_Products_UnitPrice" CHECK (UnitPrice >= 0),
	CONSTRAINT "CK_ReorderLevel" CHECK (ReorderLevel >= 0),
	CONSTRAINT "CK_UnitsInStock" CHECK (UnitsInStock >= 0),
	CONSTRAINT "CK_UnitsOnOrder" CHECK (UnitsOnOrder >= 0)
);
CREATE INDEX "CategoriesProducts" ON "Products"("CategoryID");
CREATE INDEX "CategoryID" ON "Products"("CategoryID");
CREATE INDEX "ProductName" ON "Products"("ProductName");
CREATE INDEX "SupplierID" ON "Products"("SupplierID");
CREATE INDEX "SuppliersProducts" ON "Products"("SupplierID");

INSERT INTO "Categories"("CategoryID","CategoryName","Description","Picture") 
VALUES(1,'Beverages','Soft drinks, coffees, teas, beers, and ales',null),
(2,'Condiments','Sweet and savory sauces, relishes, spreads, and seasonings',null),
(3,'Confections','Desserts, candies, and sweet breads',null),
(4,'Dairy Products','Cheeses',null),
(5,'Grains/Cereals','Breads, crackers, pasta, and cereal',null),
(6,'Meat/Poultry','Prepared meats',null),
(7,'Produce','Dried fruit and bean curd',null),
(8,'Seafood','Seaweed and fish',null);