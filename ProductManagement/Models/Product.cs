using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProductManagement.Models
{
    public class Product
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        public Category Category { get; set; } //Navigation Property

        [Display(Name = "Category")]
        [Required(ErrorMessage = "{0} is required.")]
        public byte CategoryId { get; set; }  //Foreign Key 

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(250)]
        [Display(Name = "Short Description")]
        public string ShortDesc { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(1000)]
        [Display(Name = "Long Description")]
        public string LongDesc { get; set; }

        [ScaffoldColumn(false)]
        public string ProductSmallImagePath { get; set; }

        [NotMapped]
        [FileExtensions("jpg,jpeg,png", ErrorMessage = "Only .jpg, .jpeg, and .png extensions are allowed.")]
        [Display(Name = "Product Small Image")]
        public HttpPostedFileBase ProductSmallImage { get; set; }

        [ScaffoldColumn(false)]
        public string ProductLargeImagePath { get; set; }

        [NotMapped]
        [FileExtensions("jpg,jpeg,png", ErrorMessage = "Only .jpg, .jpeg, and .png extensions are allowed.")]
        [Display(Name = "Product Large Image")]
        public HttpPostedFileBase ProductLargeImage { get; set; }


        //used for Category DropDownList
        [NotMapped]
        public SelectList CategoryList { get; set; }
    }
}