# Demonstratie Cerinte Proiect

---

## BACKEND

---

### Repository Pattern / ORM

**Entity Framework Core** ca ORM. Repository-ul separă accesul la date de logica business:

```csharp
// Interfata — contract abstracta
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
}

// Implementarea — foloseste EF Core
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.Id == id);
}
```

Inregistrat in `Program.cs`:
```csharp
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

---

### Minim 5 Entitati

Proiectul contine **5 entitati** principale:

| Entitate | Fisier | Descriere |
|---|---|---|
| `User` | `OrderService/Models/User.cs` | Utilizatori cu autentificare |
| `Order` | `OrderService/Models/Order.cs` | Comenzi de cumparare/inchiriere |
| `Payment` | `OrderService/Models/Payment.cs` | Plati asociate comenzilor |
| `Feedback` | `OrderService/Models/Feedback.cs` | Feedback utilizatori |
| `Product` | `ProductService/Models/Product.cs` | Produse din catalog |

---

### Relatii Entitati

**One-to-Many** — Un User are mai multe Orders:
```csharp
// User.cs
public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

// Order.cs
public int UserId { get; set; }
public virtual User User { get; set; } = null!;
```

**One-to-One** — O Order are un singur Payment:
```csharp
// Order.cs
public virtual Payment? Payment { get; set; }

// Payment.cs
public int OrderId { get; set; }  // FK unic -> relatie 1:1
public virtual Order Order { get; set; } = null!;
```

**One-to-Many** — Un User are mai multe Feedbacks:
```csharp
// User.cs
public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

// Feedback.cs
public int UserId { get; set; }
public virtual User User { get; set; } = null!;
```

---

### Configurare Relatii cu FluentAPI

Configurate in `ApplicationDbContext.cs` folosind Fluent API:

```csharp
// One-to-Many: User -> Orders (cascade delete)
modelBuilder.Entity<User>(entity =>
{
    entity.HasMany(e => e.Orders)
        .WithOne(e => e.User)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasMany(e => e.Feedbacks)
        .WithOne(e => e.User)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});

// One-to-One: Order -> Payment (cascade delete)
modelBuilder.Entity<Order>(entity =>
{
    entity.HasOne(e => e.Payment)
        .WithOne(e => e.Order)
        .HasForeignKey<Payment>(e => e.OrderId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

Suplimentar, atribute pe modele:
```csharp
[Table("users")]
public class User
{
    [Key] public int Id { get; set; }
    [Required][MaxLength(100)] public string Username { get; set; } = string.Empty;
    [Required][MaxLength(200)][EmailAddress] public string Email { get; set; } = string.Empty;
}
```

---

### JWT Authentication

Login genereaza token JWT semnat cu HmacSha256:

```csharp
// UserService.cs — AuthenticateAsync
public async Task<string> AuthenticateAsync(string username, string password)
{
    var user = await _repository.GetByUsernameAsync(username);
    if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        throw new UnauthorizedAccessException("Invalid username or password");

    return GenerateJwtToken(user);
}

private string GenerateJwtToken(User user)
{
    var key = Encoding.UTF8.GetBytes(secretKey);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "user"),
        }),
        Expires = DateTime.UtcNow.AddMinutes(60),
        Issuer = "ShopPlatform",
        Audience = "ShopPlatformUsers",
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };
    return new JwtSecurityTokenHandler().WriteToken(
        new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
}
```

Parola stocata cu BCrypt hash:
```csharp
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
```

---

### Permisiuni Bazate pe Roluri

Verificare rol in controller prin `[Authorize(Roles = "admin")]`:

```csharp
// ProductsController.cs (ProductService)
[HttpPost]
[Authorize(Roles = "admin")]   // doar adminii pot adauga produse
public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto) { ... }

[HttpDelete("{id}")]
[Authorize(Roles = "admin")]   // doar adminii pot sterge produse
public async Task<IActionResult> DeleteProduct(int id) { ... }

[HttpGet]
[AllowAnonymous]               // oricine poate vedea produsele
public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts() { ... }
```

---

### Autorizare Controllere

Toate endpoint-urile protejate folosesc `[Authorize]`. Endpoint-urile publice au `[AllowAnonymous]`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    [Authorize]                  // necesita token JWT valid
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers() { ... }

    [HttpPost("register")]
    // fara [Authorize] = public
    public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto dto) { ... }

    [HttpPost("login")]
    // fara [Authorize] = public
    public async Task<ActionResult> Login([FromBody] LoginDto loginDto) { ... }

    [HttpDelete("{id}")]
    [Authorize]                  // necesita token JWT valid
    public async Task<IActionResult> DeleteUser(int id) { ... }
}
```

---

### CRUD Controllers

Exemplu complet pe `OrdersController`:

```csharp
[HttpGet]                          // GET /api/orders — lista
[Authorize]
public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders() { ... }

[HttpGet("{id}")]                  // GET /api/orders/5 — dupa ID
[Authorize]
public async Task<ActionResult<OrderDto>> GetOrderById(int id) { ... }

[HttpGet("user/{userId}")]         // GET /api/orders/user/3 — filtrare extra
[Authorize]
public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUserId(int userId) { ... }

[HttpPost]                         // POST /api/orders — creare
[Authorize]
public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto) { ... }

[HttpPatch("{id}/status")]         // PATCH /api/orders/5/status — update partial
[Authorize]
public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] OrderStatus status) { ... }

[HttpDelete("{id}")]               // DELETE /api/orders/5 — stergere
[Authorize]
public async Task<IActionResult> DeleteOrder(int id) { ... }
```

---

### Error Handling

Middleware global in `ExceptionMiddleware.cs` captureaza toate exceptiile si returneaza status codes corecte:

```csharp
private static Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    var statusCode = exception switch
    {
        KeyNotFoundException       => HttpStatusCode.NotFound,          // 404
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,     // 401
        ArgumentException          => HttpStatusCode.BadRequest,        // 400
        InvalidOperationException  => HttpStatusCode.BadRequest,        // 400
        _                          => HttpStatusCode.InternalServerError // 500
    };

    context.Response.StatusCode = (int)statusCode;
    var response = new
    {
        statusCode = context.Response.StatusCode,
        message = exception.Message,
        details = exception.InnerException?.Message
    };
    return context.Response.WriteAsync(JsonSerializer.Serialize(response));
}
```

Inregistrat in `Program.cs` inainte de toate middleware-urile:
```csharp
app.UseMiddleware<ExceptionMiddleware>();
```

---

### DTO-uri

DTO-uri separate pentru request si response — entitatea `User` nu se expune direct:

```csharp
// Request DTO — ce primeste API-ul la creare
public class CreateUserDto
{
    [Required][MaxLength(100)] public string Username { get; set; } = string.Empty;
    [Required][EmailAddress]   public string Email    { get; set; } = string.Empty;
    [Required][MinLength(6)]   public string Password { get; set; } = string.Empty;
    public string? FullName  { get; set; }
    public string? Address   { get; set; }
}

// Response DTO — ce returneaza API-ul (fara PasswordHash!)
public class UserDto
{
    public int      Id        { get; set; }
    public string   Username  { get; set; } = string.Empty;
    public string   Email     { get; set; } = string.Empty;
    public string?  FullName  { get; set; }
    public bool     IsActive  { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Mapare automata prin AutoMapper:
```csharp
// AutoMapperProfile.cs
CreateMap<User, UserDto>();
CreateMap<CreateUserDto, User>()
    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
    .ForMember(dest => dest.Id,           opt => opt.Ignore());
```

Utilizat in controller:
```csharp
[HttpPost("register")]
public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto userDto)
{
    var user = await _userService.CreateUserAsync(userDto); // returneaza UserDto
    return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
}
```

---

### Notificare Email (MailKit)

Email trimis automat la crearea unei comenzi, implementat cu MailKit (SMTP):

```csharp
// EmailService.cs
public async Task SendOrderConfirmationAsync(Order order)
{
    var subject = $"Order Confirmation - Order #{order.Id}";
    var body = $@"
        <h2>Order Confirmation</h2>
        <p>Thank you for your order!</p>
        <p><strong>Order ID:</strong> {order.Id}</p>
        <p><strong>Total Price:</strong> ${order.TotalPrice:F2}</p>
        <p><strong>Status:</strong> {order.Status}</p>
    ";
    await SendEmailAsync("customer@example.com", subject, body);
}

public async Task SendEmailAsync(string to, string subject, string body)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("Shop Platform", fromEmail));
    message.To.Add(MailboxAddress.Parse(to));
    message.Subject = subject;
    message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(smtpUsername, smtpPassword);
    await client.SendAsync(message);
}
```

Apelat in `OrderService.cs` la crearea comenzii:
```csharp
public async Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto)
{
    var order = _mapper.Map<Order>(orderDto);
    order.Status = OrderStatus.Pending;
    var created = await _repository.CreateAsync(order);

    // Email de confirmare trimis non-blocking
    _ = Task.Run(async () =>
        await _emailService.SendOrderConfirmationAsync(created));

    return _mapper.Map<OrderDto>(created);
}
```

---

---

## FRONTEND

---

### Formular Register

`RegisterPage.tsx` — formular cu validare Formik + Yup:

```tsx
const formik = useFormik({
  initialValues: {
    username: '', email: '', password: '', confirmPassword: '',
    firstName: '', lastName: '',
  },
  validationSchema: registerSchema, // Yup schema cu validare parola
  onSubmit: async (values) => {
    await authService.register({ username, email, password, firstName, lastName });
    navigate('/login');
  },
});

// Campuri in formular:
<TextField name="username" label="Username" required ... />
<TextField name="email"    label="Email"    type="email" required ... />
<TextField name="firstName" label="First Name" ... />
<TextField name="lastName"  label="Last Name"  ... />
<TextField name="password"        label="Password"         type="password" required ... />
<TextField name="confirmPassword" label="Confirm Password" type="password" required ... />
```

---

### Routing si NavBar

`App.tsx` — rute definite cu React Router:

```tsx
<Routes>
  <Route path="/"               element={<HomePage />} />
  <Route path="/login"          element={<LoginPage />} />
  <Route path="/register"       element={<RegisterPage />} />
  <Route path="/products"       element={<ProductsPage />} />
  <Route path="/products/:id"   element={<ProductDetailPage />} />
  <Route path="/orders"         element={<ProtectedRoute roles={[Role.USER, Role.ADMIN]}><OrdersPage /></ProtectedRoute>} />
  <Route path="/feedback"       element={<ProtectedRoute roles={[Role.USER, Role.ADMIN]}><FeedbackPage /></ProtectedRoute>} />
  <Route path="/admin/products" element={<ProtectedRoute roles={[Role.ADMIN]}><AdminProductsPage /></ProtectedRoute>} />
  <Route path="*"               element={<NotFoundPage />} />
</Routes>
```

`Navbar.tsx` — navigare cu link-uri catre toate paginile + logout.

**Pagini (minim 3):** Home, Products, Orders, Feedback, Admin Products

---

### Tabel cu Paginare si Cautare

`ProductsPage.tsx` — tabel cu 4+ coloane, cautare si paginare:

```tsx
// Cautare
const [searchQuery, setSearchQuery] = useState('');
const handleSearch = () => {
  setPage(0);
  setAppliedSearch(searchQuery);
};

// Paginare
const [page, setPage] = useState(0);
const [rowsPerPage, setRowsPerPage] = useState(10);

// Query cu search + paginare
const { data } = useQuery({
  queryKey: ['products', page, rowsPerPage, appliedSearch],
  queryFn: () => appliedSearch
    ? productService.searchProducts({ page, size: rowsPerPage, query: appliedSearch })
    : productService.getProducts({ page, size: rowsPerPage }),
});

// Tabel cu 4 coloane
<TableHead>
  <TableRow>
    <TableCell>Name</TableCell>
    <TableCell>Category</TableCell>
    <TableCell>Price</TableCell>
    <TableCell>Stock</TableCell>
  </TableRow>
</TableHead>

// Componenta de paginare MUI
<TablePagination
  count={totalElements}
  page={page}
  rowsPerPage={rowsPerPage}
  onPageChange={handleChangePage}
  onRowsPerPageChange={handleChangeRowsPerPage}
/>
```

---

### Add / Edit / Delete cu Dialog de Confirmare

`AdminProductsPage.tsx` — CRUD complet cu dialog de confirmare la stergere:

```tsx
// State pentru dialog de stergere
const [deleteDialogOpen, setDeleteDialogOpen]   = useState(false);
const [productToDelete,  setProductToDelete]    = useState<Product | null>(null);

// Deschide dialogul
const handleDeleteClick = (product: Product) => {
  setProductToDelete(product);
  setDeleteDialogOpen(true);
};

// Dialog de confirmare
<Dialog open={deleteDialogOpen}>
  <DialogTitle>Confirm Delete</DialogTitle>
  <DialogContent>
    <DialogContentText>
      Are you sure you want to delete "{productToDelete?.name}"?
    </DialogContentText>
  </DialogContent>
  <DialogActions>
    <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
    <Button color="error" onClick={handleDeleteConfirm}>Delete</Button>
  </DialogActions>
</Dialog>

// Butoane in tabel
<IconButton onClick={() => handleEditClick(product)}><Edit /></IconButton>
<IconButton onClick={() => handleDeleteClick(product)} color="error"><Delete /></IconButton>
```

---

### Formular Feedback

`FeedbackPage.tsx` — formular cu toate elementele cerute, conectat la backend:

```tsx
// SELECT — categorie
<Select name="category" label="Category" value={formik.values.category} onChange={formik.handleChange}>
  <MenuItem value={FeedbackCategory.PRODUCT_QUALITY}>Product Quality</MenuItem>
  <MenuItem value={FeedbackCategory.DELIVERY}>Delivery</MenuItem>
  <MenuItem value={FeedbackCategory.CUSTOMER_SERVICE}>Customer Service</MenuItem>
  <MenuItem value={FeedbackCategory.WEBSITE}>Website</MenuItem>
</Select>

// RADIO BUTTONS — rating 1-5
<RadioGroup row name="rating" value={String(formik.values.rating)}
  onChange={(e) => formik.setFieldValue('rating', Number(e.target.value))}>
  {[1, 2, 3, 4, 5].map((value) => (
    <FormControlLabel key={value} value={String(value)} control={<Radio />} label={String(value)} />
  ))}
</RadioGroup>

// TEXTAREA — comentariu
<TextField name="comment" label="Comment" multiline rows={4} ... />

// CHECKBOX — acord termeni
<FormControlLabel
  control={<Checkbox name="agreedToTerms" checked={formik.values.agreedToTerms} onChange={formik.handleChange} />}
  label="I agree to the terms and conditions"
/>

// Submit conectat la backend
const submitMutation = useMutation({
  mutationFn: feedbackService.submitFeedback,  // POST /api/feedback
});
```

---

## BONUS

| Bonus | Implementat |
|---|---|
| Docker / docker-compose | `docker-compose.yml` cu servicii backend, frontend, postgres |
| Stripe payments | Integrare completa cu PaymentIntent si Refund |
| Elasticsearch | Cautare full-text produse in ProductService |
| Yarp API Gateway | Reverse proxy pentru rutare microservicii |
| FluentValidation | Validare avansata comenzi si feedback |
| Health Checks | Endpoint `/health` cu verificare PostgreSQL |
