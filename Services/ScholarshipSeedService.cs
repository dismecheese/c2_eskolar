using c2_eskolar.Data;
using c2_eskolar.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace c2_eskolar.Services
{
	// SERVICE TO SEED SAMPLE SCHOLARSHIPS INTO DATABASE
	public class ScholarshipSeedService
	{
		private readonly ApplicationDbContext _context;

		public ScholarshipSeedService(ApplicationDbContext context)
		{
			_context = context;
		}

		// SEEDS DEMO SCHOLARSHIPS IF NONE EXIST
		public async Task SeedSampleScholarshipsAsync()
		{
			if (await _context.Scholarships.AnyAsync())
			{
				return; // Skip seeding if data already present
			}

			var sampleScholarships = new List<Scholarship>
			{
				new Scholarship
				{
					Title = "Tech Excellence Scholarship",
					Description = "Full tuition coverage for outstanding students in STEM fields. Includes mentorship and internship opportunities.",
					Benefits = "₱50,000 tuition + laptop + mentorship program",
					MonetaryValue = 50000,
					ApplicationDeadline = new DateTime(2025, 1, 15),
					Requirements = "Must be enrolled in a STEM course. Minimum GPA: 3.5. Submit transcript and essay.",
					SlotsAvailable = 10,
					MinimumGPA = 3.5M,
					RequiredCourse = "STEM",
					RequiredYearLevel = 2,
					RequiredUniversity = "University of Technology",
					IsActive = true,
					IsInternal = false,
					ExternalApplicationUrl = "https://techscholarships.org/apply",
					CreatedAt = DateTime.UtcNow.AddDays(-10)
				},
				new Scholarship
				{
					Title = "Academic Merit Scholarship",
					Description = "Awarded to students with exceptional academic performance. Covers tuition and provides a stipend.",
					Benefits = "₱30,000 tuition + ₱5,000 monthly stipend",
					MonetaryValue = 30000,
					ApplicationDeadline = new DateTime(2025, 2, 1),
					Requirements = "Minimum GPA: 3.8. Must submit recommendation letter.",
					SlotsAvailable = 5,
					MinimumGPA = 3.8M,
					RequiredCourse = null,
					RequiredYearLevel = null,
					RequiredUniversity = null,
					IsActive = true,
					IsInternal = false,
					CreatedAt = DateTime.UtcNow.AddDays(-7)
				},
				new Scholarship
				{
					Title = "Community Service Award",
					Description = "For students who have demonstrated exceptional commitment to community service. Includes cash award and recognition.",
					Benefits = "₱2,000 cash award + graduation recognition",
					MonetaryValue = 2000,
					ApplicationDeadline = new DateTime(2025, 3, 1),
					Requirements = "Submit proof of community service and essay.",
					SlotsAvailable = 3,
					MinimumGPA = 2.5M,
					RequiredCourse = null,
					RequiredYearLevel = null,
					RequiredUniversity = null,
					IsActive = true,
					IsInternal = false,
					CreatedAt = DateTime.UtcNow.AddDays(-3)
				},
				new Scholarship
				{
					Title = "STEM Leadership Grant",
					Description = "Grant for STEM students showing leadership in research and community involvement. Requires mid-term report.",
					Benefits = "₱20,000 grant + research funding",
					MonetaryValue = 20000,
					ApplicationDeadline = new DateTime(2025, 1, 30),
					Requirements = "STEM major. Submit leadership statement and project proposal.",
					SlotsAvailable = 2,
					MinimumGPA = 3.2M,
					RequiredCourse = "STEM",
					RequiredYearLevel = 3,
					RequiredUniversity = null,
					IsActive = true,
					IsInternal = false,
					CreatedAt = DateTime.UtcNow.AddDays(-1)
				}
			};

			_context.Scholarships.AddRange(sampleScholarships);
			await _context.SaveChangesAsync();
		}
	}
}
