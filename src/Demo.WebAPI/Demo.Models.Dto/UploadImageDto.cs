using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Demo.Models.Dto {
    public class UploadImageDto {

        /// <summary>
        /// Тело документа
        /// </summary>
        [Required]
        public IFormFile Body { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        [Required]
        [RegularExpression(".*\\.(png|bmp|emf|exif|gif|icon|jpe?g|tiff?|webp)$", 
            ErrorMessage = "Убедитесь, что загружаемый файл имеет одно из следующих расширений: png, bmp, emf, exif, gif, icon, jpe?g, tiff?, webp")]
        public string FileName { get; set; }
    }
}
