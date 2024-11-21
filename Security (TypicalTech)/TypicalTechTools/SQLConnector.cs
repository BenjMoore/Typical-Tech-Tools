using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;

namespace TypicalTechTools
{
    public class SQLConnector
    {
        public string ConnectionString;
        private string DboConnectionString;
       

        public SQLConnector()
        {
            // This is for before creation
            this.ConnectionString = "Data Source=localhost;Integrated Security=True;TrustServerCertificate=True";
            // This is for after
            this.DboConnectionString = "Data Source=localhost;Initial Catalog=TotalTools;Integrated Security=True;TrustServerCertificate=True";
            InitializeDatabase();
            SeedDatabase();
        }
        // Seed Database if empty
        public void SeedDatabase()
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();

                // Check if tables are empty

                if (IsTableEmpty(connection, "Products"))
                {
                    InsertProducts(connection);
                }
                if (IsTableEmpty(connection, "Comments"))
                {
                    InsertComments(connection);
                }
                if (IsTableEmpty(connection, "WarrantyFiles"))
                {
                    InsertWarrantyFiles(connection);
                }

                if (IsTableEmpty(connection, "Login"))
                {
                    InsertUsers(connection);
                }
            }
        }
        // Flag to check if the table is empty
        private bool IsTableEmpty(SqlConnection connection, string tableName)
        {
            string query = $"SELECT COUNT(*) FROM {tableName}";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                int count = (int)command.ExecuteScalar();
                return count == 0;
            }
        }
        // insert sql function to insert Users if null

        private void InsertUsers(SqlConnection connection)
        {
            string adminPassword = BCrypt.Net.BCrypt.HashPassword("Test");
            string userPassword = BCrypt.Net.BCrypt.HashPassword("Test");
            string guestPassword = BCrypt.Net.BCrypt.HashPassword("Test");

            string query = @"
            INSERT INTO Login (UserName, Password, AccessLevel, Role)
            VALUES
                ('Admin', @AdminPassword, '0', 'Admin'),
                ('Username', @UserPassword, '1','User'),
                ('Guest', @guestPassword, '2','Guest');
            ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AdminPassword", adminPassword);
                command.Parameters.AddWithValue("@UserPassword", userPassword);
                command.Parameters.AddWithValue("@guestPassword", guestPassword);
                command.ExecuteNonQuery();
            }
        }
        // insert sql function to insert Comments if null

        private void InsertComments(SqlConnection connection)
        {
            string query = @"
            INSERT INTO Comments (product_code,comment_text, session_id, created_date)
            VALUES
                (1234,'This is a great product. Highly Recommended.', NEWID(), GETDATE()),
                (1235,'Not worth the excessive price. Stick with a cheaper generic one.', NEWID(), GETDATE()),
                (1236,'A great budget buy. As good as some of the expensive alternatives.', NEWID(), GETDATE()),
                (1237,'Total garbage. Never buying this brand again.', NEWID(), GETDATE());
        ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        // insert sql function to insert Products if null

        private void InsertProducts(SqlConnection connection)
        {
            string query = @"
            INSERT INTO Products (product_code,product_name, product_price, product_description, updated_date)
            VALUES
                (1234,'Generic Headphones', 84.99, 'bluetooth headphones with fair battery life and a 1 month warranty', GETDATE()),
                (1235,'Expensive Headphones', 149.99, 'bluetooth headphones with good battery life and a 6 month warranty', GETDATE()),
                (1236,'Name Brand Headphones', 199.99, 'bluetooth headphones with good battery life and a 12 month warranty', GETDATE()),
                (1237,'Generic Wireless Mouse', 39.99, 'simple bluetooth pointing device', GETDATE()),
                (1238,'Logitach Mouse and Keyboard', 73.99, 'mouse and keyboard wired combination', GETDATE()),
                (1239,'Logitach Wireless Mouse', 149.99, 'quality wireless mouse', GETDATE());
        ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        // insert sql function to insert warantee files if null
        private void InsertWarrantyFiles(SqlConnection connection)
        {
            string query = @"
            IF NOT EXISTS (SELECT * FROM WarrantyFiles WHERE FileName = 'TypicalTools_WarrantyForm.docx.enc')
            BEGIN
                INSERT INTO WarrantyFiles (FileName, FilePath, UploadedDate)
                VALUES ('TypicalTools_WarrantyForm.docx.enc', 'wwwroot/Uploads/TypicalTools_WarrantyForm.docx.enc', GETDATE());
            END;"
            ;

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        // Initialises and creates database and tables
        public void InitializeDatabase()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string createDatabaseQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TotalTools')
                    BEGIN
                        CREATE DATABASE TotalTools;
                    END";

                using (SqlCommand command = new SqlCommand(createDatabaseQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string createTablesQuery = @"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' and xtype='U')
        BEGIN
            CREATE TABLE Products (
                product_code NVARCHAR(50) PRIMARY KEY,
                product_name NVARCHAR(100),
                product_price DECIMAL(18, 2),
                product_description NVARCHAR(MAX),
                updated_date DATETIME
            );
        END;

        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Comments' and xtype='U')
        BEGIN
            CREATE TABLE Comments (
                commentId INT PRIMARY KEY IDENTITY,
                comment_text NVARCHAR(MAX),
                product_code NVARCHAR(50),
                session_id NVARCHAR(50),
                created_date DATETIME,
                FOREIGN KEY (product_code) REFERENCES Products(product_code)
            );
        END;

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WarrantyFiles' and xtype='U')
            BEGIN
                CREATE TABLE WarrantyFiles (
                    Id INT PRIMARY KEY IDENTITY,
                    FileName NVARCHAR(255),
                    FilePath NVARCHAR(255),
                    UploadedDate DATETIME
                );
            END;  
           IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Login' and xtype='U')
                BEGIN
                    CREATE TABLE Login (
                        UserID INT PRIMARY KEY IDENTITY,
                        UserName VARCHAR(50) NOT NULL,
                        Password NVARCHAR(MAX) NOT NULL,
                        AccessLevel VARCHAR(50) NOT NULL,
                        Role VARCHAR(50) NOT NULL
                    );

                   
                END;
                ";

                using (SqlCommand command = new SqlCommand(createTablesQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        // Validates user credentials 
        public bool ValidateAdminUser(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "SELECT Password FROM Login WHERE UserName = @UserName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserName", username);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHashedPassword = reader["Password"].ToString();
                            return BCrypt.Net.BCrypt.Verify(password, storedHashedPassword);
                        }
                    }
                }
            }

            return false;
        }
        // gets admin user returns DTO AdminUser
        public AdminUser GetAdminUser(string username)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "SELECT UserName, Password, UserID, AccessLevel, Role FROM Login WHERE UserName = @UserName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserName", username);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AdminUser
                            {
                                UserName = reader["UserName"].ToString(),
                                Password = reader["Password"].ToString(),
                                UserID = Convert.ToInt32(reader["UserID"]),
                                AccessLevel = Convert.ToInt32(reader["AccessLevel"]),
                                Role = reader["Role"].ToString()
                            };
                        }
                    }
                }
            }

            return null; // Return null if the user is not found
        }
        // Creates new user, isnt currently in use but works 
        public void CreateAdminUser(AdminUser user)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO Login (UserName, Password, UserID, AccessLevel) VALUES (@UserName, @Password, @UserID, @AccessLevel)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    command.Parameters.AddWithValue("@UserName", user.UserName);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@UserID", user.UserID);
                    command.Parameters.AddWithValue("@AccessLevel", user.AccessLevel);

                    command.ExecuteNonQuery();
                }
            }
        }
    

        // add product
        public void AddProduct(Product product)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO Products (product_code, product_name, product_price, product_description, updated_date)
                    VALUES (@ProductCode, @ProductName, @ProductPrice, @ProductDescription, @UpdatedDate)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@ProductPrice", product.ProductPrice);
                    command.Parameters.AddWithValue("@ProductDescription", product.ProductDescription);
                    command.Parameters.AddWithValue("@UpdatedDate", product.UpdatedDate);
                    command.ExecuteNonQuery();
                }
            }
        }
        // remove product 
        public bool RemoveProduct(int productCode)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM Products WHERE product_code = @ProductCode";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", productCode);
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }
        // Update product code
        public void UpdateProduct(Product product)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE Products
                    SET product_name = @ProductName,
                        product_price = @ProductPrice,
                        product_description = @ProductDescription,
                        updated_date = @UpdatedDate
                    WHERE product_code = @ProductCode";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@ProductPrice", product.ProductPrice);
                    command.Parameters.AddWithValue("@ProductDescription", product.ProductDescription);
                    command.Parameters.AddWithValue("@UpdatedDate", product.UpdatedDate);
                    command.ExecuteNonQuery();
                }
            }
        }
        // Get Product By product code 
        public Product GetProductByCode(string productCode)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "SELECT product_code, product_name, product_price, product_description, updated_date FROM Products WHERE product_code = @ProductCode";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", productCode);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                ProductCode = reader["product_code"].ToString(),
                                ProductName = reader["product_name"].ToString(),
                                ProductPrice = (decimal)reader["product_price"],
                                ProductDescription = reader["product_description"].ToString(),
                                UpdatedDate = (DateTime)reader["updated_date"]
                            };
                        }
                        else
                        {
                            // Handle the case where the product is not found
                            return null;
                        }
                    }
                }
            }
        }
        // Get Single Product
        public Product GetProduct(string productCode)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products WHERE product_code = @ProductCode";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", productCode);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                ProductCode = reader["product_code"].ToString(),
                                ProductName = reader["product_name"].ToString(),
                                ProductPrice = decimal.Parse(reader["product_price"].ToString()),
                                ProductDescription = reader["product_description"].ToString(),
                                UpdatedDate = DateTime.Parse(reader["updated_date"].ToString())
                            };
                        }
                    }
                }
            }
            return null;
        }
        // Gets All Products, returnts list of type product
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductCode = reader["product_code"].ToString(),
                                ProductName = reader["product_name"].ToString(),
                                ProductPrice = decimal.Parse(reader["product_price"].ToString()),
                                ProductDescription = reader["product_description"].ToString(),
                                UpdatedDate = DateTime.Parse(reader["updated_date"].ToString())
                            });
                        }
                    }
                }
            }
            return products;
        }

        // adds comment, takes dto comment
        public void AddComment(Comment comment)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                // Check if the product exists
                string checkProductQuery = "SELECT COUNT(*) FROM Products WHERE product_code = @ProductCode";
                using (SqlCommand checkCommand = new SqlCommand(checkProductQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@ProductCode", comment.ProductCode);
                    int productExists = (int)checkCommand.ExecuteScalar();
                    if (productExists == 0)
                    {
                        throw new Exception("Product code does not exist.");
                    }
                }

                // Insert the comment
                string query = @"
                INSERT INTO Comments (comment_text, product_code, session_id, created_date)
                VALUES (@CommentText, @ProductCode, @SessionId, @CreatedDate)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CommentText", comment.CommentText);
                    command.Parameters.AddWithValue("@ProductCode", comment.ProductCode);
                    command.Parameters.AddWithValue("@SessionId", comment.UserID);
                    command.Parameters.AddWithValue("@CreatedDate", comment.CreatedDate);
                    command.ExecuteNonQuery();
                }
            }
        }
        // Gets the comments of a product and returns to a list of type comment
        public List<Comment> GetCommentsForProduct(string productCode)
        {
            var comments = new List<Comment>();
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Comments WHERE product_code = @ProductCode";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", productCode);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var comment = new Comment
                            {
                                CommentId = reader.GetInt32(0),
                                CommentText = reader.GetString(1),
                                ProductCode = reader.GetString(2),
                                UserID = reader.GetString(3),
                                CreatedDate = reader.GetDateTime(4)
                            };
                            comments.Add(comment);
                        }
                    }
                }
            }
            return comments;
        }
        //Edit comment Takes DTO Comment
        public void EditComment(Comment comment)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                comment.CreatedDate = DateTime.Now;
                connection.Open();
                string query = @"
                UPDATE Comments
                SET comment_text = @CommentText, created_date = @CreatedDate
                WHERE commentId = @CommentId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CommentText", comment.CommentText);
                    command.Parameters.AddWithValue("@CreatedDate", comment.CreatedDate);
                    command.Parameters.AddWithValue("@CommentId", comment.CommentId);
                    command.ExecuteNonQuery();
                }
            }
        }
        // Get Comment By ID
        public Comment GetComment(int commentId)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Comments WHERE commentId = @CommentId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CommentId", commentId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Comment
                            {
                                CommentId = reader.GetInt32(0),
                                CommentText = reader.GetString(1),
                                ProductCode = reader.GetString(2),
                                UserID = reader.GetString(3),
                                CreatedDate = reader.GetDateTime(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Deletes comment by ID
        public void DeleteComment(int commentId)
        {
            using (SqlConnection connection = new SqlConnection(DboConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM Comments WHERE commentId = @CommentId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CommentId", commentId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Adds a Waranty File
        public void AddWarrantyFile(FileModel file)
        {
            using (var connection = new SqlConnection(DboConnectionString))
            {
                string query = "INSERT INTO WarrantyFiles (FileName, FilePath, UploadedDate) VALUES (@FileName, @FilePath, @UploadedDate)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FileName", file.FileName);
                    command.Parameters.AddWithValue("@FilePath", file.FilePath);
                    command.Parameters.AddWithValue("@UploadedDate", file.UploadedDate);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Returns a List of type FileModel with all current warantee files
        public List<FileModel> GetWarrantyFiles()
        {
            var files = new List<FileModel>();
            using (var connection = new SqlConnection(DboConnectionString))
            {
                string query = "SELECT * FROM WarrantyFiles";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            files.Add(new FileModel
                            {
                                Id = reader.GetInt32(0),
                                FileName = reader.GetString(1),
                                FilePath = reader.GetString(2),
                                UploadedDate = reader.GetDateTime(3)
                            });
                        }
                    }
                }
            }
            return files;
        }

        // Gets Warranty File By ID
        public FileModel GetWarrantyFileById(int id)
        {
            using (var connection = new SqlConnection(DboConnectionString))
            {
                string query = "SELECT * FROM WarrantyFiles WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new FileModel
                            {
                                Id = reader.GetInt32(0),
                                FileName = reader.GetString(1),
                                FilePath = reader.GetString(2),
                                UploadedDate = reader.GetDateTime(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Deletes Warranty File
        public void DeleteWarrantyFile(int id)
        {
            using (var connection = new SqlConnection(DboConnectionString))
            {
                string query = "DELETE FROM WarrantyFiles WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}



