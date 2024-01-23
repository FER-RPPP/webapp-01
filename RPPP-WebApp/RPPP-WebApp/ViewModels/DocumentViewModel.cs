using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Model;

namespace RPPP_WebApp.ViewModels
{
    public class DocumentViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Format { get; set; }

        public string DocumentType { get; set; }
    }
}
