using TypicalTechTools.Models;
using System.Collections.Generic;
/* Data Access Middleware for my sql controller */
namespace TypicalTechTools.DataAccess
{
    public class DataAccessLayer
    {
        private SQLConnector _sqlConnector;

        public DataAccessLayer()
        {
            _sqlConnector = new SQLConnector();
        }
        #region Warranty
        public void AddWarrantyFile(FileModel file)
        {
            _sqlConnector.AddWarrantyFile(file);
        }
        public List<FileModel> GetWarrantyFiles()
        {
            return _sqlConnector.GetWarrantyFiles();
        }
        public FileModel GetWarrantyFileById(int id)
        {
            return _sqlConnector.GetWarrantyFileById(id);
        }
        public void DeleteWarrantyFile(int id)
        {
            _sqlConnector.DeleteWarrantyFile(id);
        }
        #endregion
        #region User
        public bool ValidateAdminUser(string userName, string passWord)
        {
            return _sqlConnector.ValidateAdminUser(userName, passWord);

        }
        public AdminUser GetAdminUser(string userName)
        {
            return _sqlConnector.GetAdminUser(userName);
        }
        public void CreateAdminUser(AdminUser adminUser) 
        {
            _sqlConnector.CreateAdminUser(adminUser);
        }
        public bool CheckUserNameExists(string userName) 
        {
            return _sqlConnector.CheckUserNameExists(userName);
        }
        #endregion
        #region Products

        public Product GetProductByCode(string productCode)
        {
            return _sqlConnector.GetProductByCode(productCode);
        }
        public List<Product> GetProducts()
        {
            return _sqlConnector.GetAllProducts();
        }

        public Product GetProduct(string productCode)
        {
            return _sqlConnector.GetProduct(productCode);
        }

        public void AddProduct(Product product)
        {
            _sqlConnector.AddProduct(product);
        }

        public void UpdateProduct(Product product)
        {
            _sqlConnector.UpdateProduct(product);
        }
        public void EditComment(Comment comment)
        {
            _sqlConnector.EditComment(comment);
        }

        public bool RemoveProduct(int productCode)
        {
            return _sqlConnector.RemoveProduct(productCode);
        }
        #endregion
        #region Comments

        public List<Comment> GetCommentsForProduct(string productCode)
        {
            return _sqlConnector.GetCommentsForProduct(productCode);
        }

        public Comment GetComment(int commentId)
        {
            return _sqlConnector.GetComment(commentId);
        }

        public void AddComment(Comment comment)
        {
            comment.CreatedDate = DateTime.Now;
            _sqlConnector.AddComment(comment);
        }
        public void DeleteComment(int commentId)
        {
            _sqlConnector.DeleteComment(commentId);
        }

        #endregion
    }
}
