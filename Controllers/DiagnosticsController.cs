// using Microsoft.EntityFrameworkCore;
// using GroceryOrderingApp.Backend.Data;
// using Microsoft.AspNetCore.Mvc;

// namespace GroceryOrderingApp.Backend.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class DiagnosticsController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly ILogger<DiagnosticsController> _logger;

//         public DiagnosticsController(ApplicationDbContext context, ILogger<DiagnosticsController> logger)
//         {
//             _context = context;
//             _logger = logger;
//         }

//         /// <summary>
//         /// Check database connection and tables status
//         /// </summary>
//         [HttpGet("status")]
//         public async Task<IActionResult> GetStatus()
//         {
//             try
//             {
//                 // Test connection
//                 var canConnect = await _context.Database.CanConnectAsync();
                
//                 if (!canConnect)
//                 {
//                     return BadRequest(new { message = "Cannot connect to database" });
//                 }

//                 // Get all tables
//                 var tables = new
//                 {
//                     roles = await _context.Roles.CountAsync(),
//                     users = await _context.Users.CountAsync(),
//                     categories = await _context.Categories.CountAsync(),
//                     products = await _context.Products.CountAsync(),
//                     orders = await _context.Orders.CountAsync(),
//                     orderItems = await _context.OrderItems.CountAsync()
//                 };

//                 return Ok(new
//                 {
//                     message = "Database connection successful",
//                     isConnected = true,
//                     tablesStatus = tables
//                 });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error checking database status");
//                 return BadRequest(new { message = ex.Message });
//             }
//         }

//         /// <summary>
//         /// Force create database schema
//         /// </summary>
//         [HttpPost("create-schema")]
//         public IActionResult CreateSchema()
//         {
//             try
//             {
//                 _logger.LogInformation("Attempting to create database schema...");
                
//                 // Ensure database and schema are created
//                 var created = _context.Database.EnsureCreated();
                
//                 if (created)
//                 {
//                     return Ok(new { message = "Database schema created successfully" });
//                 }
//                 else
//                 {
//                     return Ok(new { message = "Database schema already exists" });
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error creating schema");
//                 return BadRequest(new { message = ex.Message });
//             }
//         }

//         /// <summary>
//         /// Seed database with initial data
//         /// </summary>
//         [HttpPost("seed-data")]
//         public async Task<IActionResult> SeedData()
//         {
//             try
//             {
//                 _logger.LogInformation("Seeding database...");
                
//                 var seeder = new DatabaseSeeder(_context);
//                 await seeder.SeedAsync();
                
//                 return Ok(new { message = "Database seeded successfully" });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error seeding database");
//                 return BadRequest(new { message = ex.Message });
//             }
//         }

//         /// <summary>
//         /// Get available API endpoints for testing
//         /// </summary>
//         [HttpGet("api-endpoints")]
//         public IActionResult GetApiEndpoints()
//         {
//             var endpoints = new
//             {
//                 authentication = new
//                 {
//                     login = new
//                     {
//                         method = "POST",
//                         url = "/api/auth/login",
//                         description = "Login with admin credentials",
//                         example = new
//                         {
//                             userId = "admin",
//                             password = "Admin@123"
//                         }
//                     }
//                 },
//                 categories = new
//                 {
//                     list = new
//                     {
//                         method = "GET",
//                         url = "/api/categories",
//                         description = "List all categories"
//                     },
//                     create = new
//                     {
//                         method = "POST",
//                         url = "/api/categories",
//                         description = "Create new category (Admin only)",
//                         example = new
//                         {
//                             name = "Vegetables",
//                             isActive = true
//                         }
//                     }
//                 },
//                 products = new
//                 {
//                     list = new
//                     {
//                         method = "GET",
//                         url = "/api/products",
//                         description = "List all products"
//                     },
//                     getById = new
//                     {
//                         method = "GET",
//                         url = "/api/products/{id}",
//                         description = "Get product by ID"
//                     },
//                     create = new
//                     {
//                         method = "POST",
//                         url = "/api/products",
//                         description = "Create new product (Admin only)",
//                         example = new
//                         {
//                             name = "Tomato",
//                             description = "Fresh red tomato",
//                             price = 40,
//                             stockQuantity = 100,
//                             categoryId = 1
//                         }
//                     },
//                     update = new
//                     {
//                         method = "PUT",
//                         url = "/api/products/{id}",
//                         description = "Update product (Admin only)",
//                         example = new
//                         {
//                             name = "Fresh Tomato",
//                             description = "Very fresh red tomato",
//                             price = 50,
//                             stockQuantity = 150,
//                             categoryId = 1,
//                             isActive = true
//                         }
//                     },
//                     delete = new
//                     {
//                         method = "DELETE",
//                         url = "/api/products/{id}",
//                         description = "Delete product (Admin only)"
//                     }
//                 },
//                 orders = new
//                 {
//                     list = new
//                     {
//                         method = "GET",
//                         url = "/api/orders",
//                         description = "List orders (requires authentication)"
//                     },
//                     create = new
//                     {
//                         method = "POST",
//                         url = "/api/orders",
//                         description = "Create new order",
//                         example = new
//                         {
//                             items = new object[]
//                             {
//                                 new
//                                 {
//                                     productId = 1,
//                                     quantity = 2,
//                                     priceAtTime = 40
//                                 }
//                             }
//                         }
//                     }
//                 }
//             };

//             return Ok(endpoints);
//         }

//         /// <summary>
//         /// Full database initialization
//         /// </summary>
//         [HttpPost("full-init")]
//         public async Task<IActionResult> FullInitialization()
//         {
//             try
//             {
//                 _logger.LogInformation("Starting full database initialization...");
                
//                 // Step 1: Create schema
//                 _logger.LogInformation("Step 1: Creating schema...");
//                 _context.Database.EnsureCreated();
                
//                 // Step 2: Check if data exists
//                 var rolesCount = await _context.Roles.CountAsync();
                
//                 if (rolesCount == 0)
//                 {
//                     _logger.LogInformation("Step 2: Seeding data...");
//                     var seeder = new DatabaseSeeder(_context);
//                     await seeder.SeedAsync();
//                 }
//                 else
//                 {
//                     _logger.LogInformation("Step 2: Data already exists, skipping seeding");
//                 }
                
//                 // Step 3: Verify
//                 var status = new
//                 {
//                     roles = await _context.Roles.CountAsync(),
//                     users = await _context.Users.CountAsync(),
//                     categories = await _context.Categories.CountAsync(),
//                     products = await _context.Products.CountAsync(),
//                     orders = await _context.Orders.CountAsync()
//                 };

//                 return Ok(new
//                 {
//                     message = "Full initialization completed successfully",
//                     tablesCreated = status
//                 });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error during full initialization");
//                 return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
//             }
//         }
//     }
// }
