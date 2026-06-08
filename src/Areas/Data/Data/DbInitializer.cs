using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Helpers;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Data;

public static class DbInitializer {
  public static async Task InitializeAsync(IServiceProvider serviceProvider) {
    using var scope = serviceProvider.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await context.Database.MigrateAsync();

    await SeedRolesAsync(roleManager);
    await SeedAdminUserAsync(userManager);
    await SeedCountriesHotelsToursAsync(context);
    await SeedTestUsersActivityAsync(context, userManager);
  }

  private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
    string[] roles =
    {
            RoleNames.Administrator,
            RoleNames.User
        };

    foreach (var role in roles) {
      bool roleExists = await roleManager.RoleExistsAsync(role);

      if (!roleExists) {
        await roleManager.CreateAsync(new IdentityRole(role));
      }
    }
  }

  private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager) {
    const string adminEmail = "admin@tour.local";
    const string adminPassword = "Admin123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null) {
      adminUser = new ApplicationUser {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true,
        FullName = "Адміністратор системи",
        RegisteredAt = DateTime.Now,
        IsBlocked = false
      };

      var result = await userManager.CreateAsync(adminUser, adminPassword);

      if (result.Succeeded) {
        await userManager.AddToRoleAsync(adminUser, RoleNames.Administrator);
      }
    }
  }

  private static async Task SeedCountriesHotelsToursAsync(ApplicationDbContext context) {
    if (await context.Countries.AnyAsync()) {
      return;
    }

    var egypt = new Country {
      Name = "Єгипет",
      Description = "Популярний туристичний напрям із теплим кліматом, морськими курортами та розвиненою готельною інфраструктурою."
    };

    var turkey = new Country {
      Name = "Туреччина",
      Description = "Напрям для пляжного, сімейного та екскурсійного відпочинку з широким вибором готелів."
    };

    var greece = new Country {
      Name = "Греція",
      Description = "Європейський туристичний напрям із морськими курортами, історичними пам’ятками та острівним відпочинком."
    };

    var spain = new Country {
      Name = "Іспанія",
      Description = "Туристичний напрям із розвиненою курортною інфраструктурою, культурними маршрутами та пляжним відпочинком."
    };

    context.Countries.AddRange(egypt, turkey, greece, spain);
    await context.SaveChangesAsync();

    var hotels = new List<Hotel>
    {
            new Hotel
            {
                Name = "Sunrise Beach Resort",
                Rating = 5,
                CountryId = egypt.Id,
                Description = "П’ятизірковий курортний готель біля моря з басейнами, ресторанами та сервісом для сімейного відпочинку."
            },
            new Hotel
            {
                Name = "Nile Garden Hotel",
                Rating = 4,
                CountryId = egypt.Id,
                Description = "Комфортний готель для спокійного відпочинку та екскурсійних програм."
            },
            new Hotel
            {
                Name = "Anatolia Premium Hotel",
                Rating = 5,
                CountryId = turkey.Id,
                Description = "Сучасний готель із концепцією all inclusive, власною пляжною зоною та розважальними програмами."
            },
            new Hotel
            {
                Name = "Blue Coast Hotel",
                Rating = 4,
                CountryId = turkey.Id,
                Description = "Готель для сімейного відпочинку поблизу морського узбережжя."
            },
            new Hotel
            {
                Name = "Aegean Sea Resort",
                Rating = 5,
                CountryId = greece.Id,
                Description = "Курортний готель із видом на Егейське море та зручною інфраструктурою для відпочинку."
            },
            new Hotel
            {
                Name = "Santorini View Hotel",
                Rating = 4,
                CountryId = greece.Id,
                Description = "Готель із мальовничими краєвидами, зручним розміщенням і спокійною атмосферою."
            },
            new Hotel
            {
                Name = "Costa Brava Palace",
                Rating = 5,
                CountryId = spain.Id,
                Description = "Комфортабельний готель на узбережжі з високим рівнем сервісу."
            },
            new Hotel
            {
                Name = "Barcelona Travel Hotel",
                Rating = 4,
                CountryId = spain.Id,
                Description = "Міський готель для поєднання екскурсійного та пляжного відпочинку."
            }
        };

    context.Hotels.AddRange(hotels);
    await context.SaveChangesAsync();

    var tours = new List<Tour>
    {
            new Tour
            {
                Title = "Морський відпочинок у Шарм-ель-Шейху",
                Description = "Тур для відпочинку на узбережжі Червоного моря з проживанням у готелі високого рівня, харчуванням і можливістю екскурсій.",
                CountryId = egypt.Id,
                HotelId = hotels[0].Id,
                Price = 28500,
                StartDate = DateTime.Today.AddDays(20),
                EndDate = DateTime.Today.AddDays(27),
                AvailablePlaces = 12,
                PhotoPath = "/uploads/tours/1.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Tour
            {
                Title = "Екскурсійний тур до Каїра",
                Description = "Програма для знайомства з історичними пам’ятками Єгипту, музеями та культурними об’єктами.",
                CountryId = egypt.Id,
                HotelId = hotels[1].Id,
                Price = 24700,
                StartDate = DateTime.Today.AddDays(30),
                EndDate = DateTime.Today.AddDays(36),
                AvailablePlaces = 8,
                PhotoPath = "/uploads/tours/2.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Tour
            {
                Title = "Сімейний відпочинок в Анталії",
                Description = "Тур до Туреччини з проживанням у готелі all inclusive, пляжною інфраструктурою та дитячими розвагами.",
                CountryId = turkey.Id,
                HotelId = hotels[2].Id,
                Price = 32100,
                StartDate = DateTime.Today.AddDays(18),
                EndDate = DateTime.Today.AddDays(25),
                AvailablePlaces = 15,
                PhotoPath = "/uploads/tours/3.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Tour
            {
                Title = "Курортний тур до Кемера",
                Description = "Пропозиція для пляжного відпочинку з комфортним розміщенням, харчуванням і можливістю активного дозвілля.",
                CountryId = turkey.Id,
                HotelId = hotels[3].Id,
                Price = 29800,
                StartDate = DateTime.Today.AddDays(24),
                EndDate = DateTime.Today.AddDays(31),
                AvailablePlaces = 10,
                PhotoPath = "/uploads/tours/4.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Tour
            {
                Title = "Відпочинок на острові Крит",
                Description = "Тур до Греції з поєднанням морського відпочинку, прогулянок історичними місцями та комфортного проживання.",
                CountryId = greece.Id,
                HotelId = hotels[4].Id,
                Price = 36700,
                StartDate = DateTime.Today.AddDays(35),
                EndDate = DateTime.Today.AddDays(42),
                AvailablePlaces = 9,
                PhotoPath = "/uploads/tours/5.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Tour
            {
                Title = "Романтичний тур на Санторіні",
                Description = "Туристична пропозиція для спокійного відпочинку з мальовничими краєвидами, прогулянками та морськими маршрутами.",
                CountryId = greece.Id,
                HotelId = hotels[5].Id,
                Price = 42500,
                StartDate = DateTime.Today.AddDays(40),
                EndDate = DateTime.Today.AddDays(47),
                AvailablePlaces = 6,
                PhotoPath = "/uploads/tours/6.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Tour
            {
                Title = "Пляжний відпочинок на Коста-Брава",
                Description = "Тур до Іспанії з проживанням на узбережжі, можливістю екскурсій і комфортною туристичною інфраструктурою.",
                CountryId = spain.Id,
                HotelId = hotels[6].Id,
                Price = 38900,
                StartDate = DateTime.Today.AddDays(28),
                EndDate = DateTime.Today.AddDays(35),
                AvailablePlaces = 11,
                PhotoPath = "/uploads/tours/7.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Tour
            {
                Title = "Барселона: місто, море та культура",
                Description = "Тур для поєднання міського відпочинку, екскурсій, пляжного дозвілля та знайомства з культурою Іспанії.",
                CountryId = spain.Id,
                HotelId = hotels[7].Id,
                Price = 41200,
                StartDate = DateTime.Today.AddDays(45),
                EndDate = DateTime.Today.AddDays(52),
                AvailablePlaces = 7,
                PhotoPath = "/uploads/tours/8.PNG",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        };

    context.Tours.AddRange(tours);
    await context.SaveChangesAsync();
  }

  private static async Task SeedTestUsersActivityAsync(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) {
    const string firstUserEmail = "user@test.local";
    const string secondUserEmail = "traveler@test.local";
    const string defaultPassword = "User123";

    var firstUser = await CreateTestUserIfNotExistsAsync(
        userManager,
        firstUserEmail,
        defaultPassword,
        "Тестовий користувач");

    var secondUser = await CreateTestUserIfNotExistsAsync(
        userManager,
        secondUserEmail,
        defaultPassword,
        "Активний турист");

    const string thirdUserEmail = "client@test.local";

    var thirdUser = await CreateTestUserIfNotExistsAsync(
        userManager,
        thirdUserEmail,
        defaultPassword,
        "Клієнт туристичної платформи");

    var egypt = await context.Countries.FirstOrDefaultAsync(c => c.Name == "Єгипет");
    var turkey = await context.Countries.FirstOrDefaultAsync(c => c.Name == "Туреччина");
    var greece = await context.Countries.FirstOrDefaultAsync(c => c.Name == "Греція");
    var spain = await context.Countries.FirstOrDefaultAsync(c => c.Name == "Іспанія");

    if (egypt == null || turkey == null || greece == null || spain == null) {
      return;
    }

    var egyptHotel = await context.Hotels.FirstOrDefaultAsync(h => h.Name == "Sunrise Beach Resort");
    var turkeyHotel = await context.Hotels.FirstOrDefaultAsync(h => h.Name == "Anatolia Premium Hotel");
    var greeceHotel = await context.Hotels.FirstOrDefaultAsync(h => h.Name == "Aegean Sea Resort");
    var spainHotel = await context.Hotels.FirstOrDefaultAsync(h => h.Name == "Costa Brava Palace");

    if (egyptHotel == null || turkeyHotel == null || greeceHotel == null || spainHotel == null) {
      return;
    }

    var historicalTours = new List<Tour>
    {
        new Tour
        {
            Title = "Архівний тур: Анталія 2024",
            Description = "Завершений туристичний тур до Туреччини з пляжним відпочинком, проживанням у готелі високого рівня та екскурсійною програмою.",
            CountryId = turkey.Id,
            HotelId = turkeyHotel.Id,
            Price = 27600,
            StartDate = new DateTime(2024, 6, 10),
            EndDate = new DateTime(2024, 6, 17),
            AvailablePlaces = 0,
            PhotoPath = "/uploads/tours/default-tour.jpg",
            IsActive = false,
            CreatedAt = new DateTime(2024, 2, 15)
        },
        new Tour
        {
            Title = "Архівний тур: Шарм-ель-Шейх 2024",
            Description = "Завершена туристична пропозиція для морського відпочинку в Єгипті з проживанням біля узбережжя Червоного моря.",
            CountryId = egypt.Id,
            HotelId = egyptHotel.Id,
            Price = 24900,
            StartDate = new DateTime(2024, 9, 5),
            EndDate = new DateTime(2024, 9, 12),
            AvailablePlaces = 0,
            PhotoPath = "/uploads/tours/default-tour.jpg",
            IsActive = false,
            CreatedAt = new DateTime(2024, 5, 20)
        },
        new Tour
        {
            Title = "Архівний тур: Крит 2025",
            Description = "Завершений тур до Греції з поєднанням морського відпочинку, історичних екскурсій та комфортного проживання.",
            CountryId = greece.Id,
            HotelId = greeceHotel.Id,
            Price = 33800,
            StartDate = new DateTime(2025, 7, 12),
            EndDate = new DateTime(2025, 7, 19),
            AvailablePlaces = 0,
            PhotoPath = "/uploads/tours/default-tour.jpg",
            IsActive = false,
            CreatedAt = new DateTime(2025, 3, 10)
        },
        new Tour
        {
            Title = "Архівний тур: Коста-Брава 2025",
            Description = "Завершений туристичний тур до Іспанії з пляжним відпочинком, міськими прогулянками та культурною програмою.",
            CountryId = spain.Id,
            HotelId = spainHotel.Id,
            Price = 36900,
            StartDate = new DateTime(2025, 8, 18),
            EndDate = new DateTime(2025, 8, 25),
            AvailablePlaces = 0,
            PhotoPath = "/uploads/tours/default-tour.jpg",
            IsActive = false,
            CreatedAt = new DateTime(2025, 4, 12)
        },
        new Tour
        {
            Title = "Архівний тур: Санторіні 2026",
            Description = "Завершений тур до Греції з акцентом на спокійний відпочинок, мальовничі краєвиди та індивідуальні екскурсії.",
            CountryId = greece.Id,
            HotelId = greeceHotel.Id,
            Price = 45200,
            StartDate = new DateTime(2026, 3, 15),
            EndDate = new DateTime(2026, 3, 22),
            AvailablePlaces = 0,
            PhotoPath = "/uploads/tours/default-tour.jpg",
            IsActive = false,
            CreatedAt = new DateTime(2026, 1, 20)
        },
        new Tour
        {
            Title = "Архівний тур: Барселона 2026",
            Description = "Завершена туристична поїздка до Іспанії з поєднанням екскурсійної програми, морського відпочинку та культурних маршрутів.",
            CountryId = spain.Id,
            HotelId = spainHotel.Id,
            Price = 39800,
            StartDate = new DateTime(2026, 4, 8),
            EndDate = new DateTime(2026, 4, 15),
            AvailablePlaces = 0,
            PhotoPath = "/uploads/tours/default-tour.jpg",
            IsActive = false,
            CreatedAt = new DateTime(2026, 2, 5)
        }
    };

    foreach (var tour in historicalTours) {
      bool tourExists = await context.Tours.AnyAsync(t => t.Title == tour.Title);

      if (!tourExists) {
        context.Tours.Add(tour);
      }
    }

    await context.SaveChangesAsync();

    var allTours = await context.Tours
        .OrderBy(t => t.StartDate)
        .ToListAsync();

    var antalya2024 = allTours.FirstOrDefault(t => t.Title == "Архівний тур: Анталія 2024");
    var egypt2024 = allTours.FirstOrDefault(t => t.Title == "Архівний тур: Шарм-ель-Шейх 2024");
    var crete2025 = allTours.FirstOrDefault(t => t.Title == "Архівний тур: Крит 2025");
    var spain2025 = allTours.FirstOrDefault(t => t.Title == "Архівний тур: Коста-Брава 2025");
    var santorini2026 = allTours.FirstOrDefault(t => t.Title == "Архівний тур: Санторіні 2026");
    var barcelona2026 = allTours.FirstOrDefault(t => t.Title == "Архівний тур: Барселона 2026");

    if (antalya2024 == null ||
        egypt2024 == null ||
        crete2025 == null ||
        spain2025 == null ||
        santorini2026 == null ||
        barcelona2026 == null) {
      return;
    }

    await AddBookingIfNotExistsAsync(context, firstUser.Id, antalya2024.Id, new DateTime(2024, 5, 21, 14, 30, 0), BookingStatuses.Confirmed);
    await AddBookingIfNotExistsAsync(context, firstUser.Id, crete2025.Id, new DateTime(2025, 6, 18, 11, 10, 0), BookingStatuses.Confirmed);
    await AddBookingIfNotExistsAsync(context, firstUser.Id, santorini2026.Id, new DateTime(2026, 2, 20, 16, 45, 0), BookingStatuses.Confirmed);

    await AddBookingIfNotExistsAsync(context, secondUser.Id, egypt2024.Id, new DateTime(2024, 8, 12, 10, 15, 0), BookingStatuses.Confirmed);
    await AddBookingIfNotExistsAsync(context, secondUser.Id, spain2025.Id, new DateTime(2025, 7, 5, 13, 20, 0), BookingStatuses.Confirmed);
    await AddBookingIfNotExistsAsync(context, secondUser.Id, barcelona2026.Id, new DateTime(2026, 3, 15, 9, 40, 0), BookingStatuses.Confirmed);

    await AddReviewIfNotExistsAsync(
        context,
        firstUser.Id,
        antalya2024.Id,
        5,
        "Тур добре підійшов для сімейного відпочинку. Готель мав зручну інфраструктуру, харчування було якісним, а організація трансферу виконана без затримок.",
        new DateTime(2024, 6, 18, 19, 20, 0));

    await AddReviewIfNotExistsAsync(
        context,
        firstUser.Id,
        crete2025.Id,
        4,
        "Поїздка на Крит залишила позитивні враження. Особливо сподобалися екскурсії та розташування готелю, хоча програма була досить насиченою.",
        new DateTime(2025, 7, 20, 18, 10, 0));

    await AddReviewIfNotExistsAsync(
        context,
        firstUser.Id,
        santorini2026.Id,
        5,
        "Санторіні повністю виправдав очікування. Тур був комфортним, краєвиди дуже гарні, а загальна організація подорожі справила приємне враження.",
        new DateTime(2026, 3, 23, 20, 5, 0));

    await AddReviewIfNotExistsAsync(
        context,
        secondUser.Id,
        egypt2024.Id,
        4,
        "Відпочинок у Єгипті був вдалим. Сподобалося море, готель і загальна атмосфера, але екскурсійна частина могла бути трохи краще структурована.",
        new DateTime(2024, 9, 13, 17, 35, 0));

    await AddReviewIfNotExistsAsync(
        context,
        secondUser.Id,
        spain2025.Id,
        5,
        "Коста-Брава залишила дуже хороші враження. Тур поєднав пляжний відпочинок і культурну програму, тому подорож була різноманітною.",
        new DateTime(2025, 8, 26, 19, 0, 0));

    await AddReviewIfNotExistsAsync(
        context,
        secondUser.Id,
        barcelona2026.Id,
        4,
        "Барселона сподобалася насиченою програмою та зручним розташуванням готелю. Загалом тур був добре організований.",
        new DateTime(2026, 4, 16, 18, 50, 0));

    await AddReviewIfNotExistsAsync(
    context,
    thirdUser.Id,
    antalya2024.Id,
    4,
    "Тур до Анталії був комфортним, особливо сподобалося розташування готелю та організація трансферу.",
    new DateTime(2024, 6, 19, 15, 25, 0));

    await AddReviewIfNotExistsAsync(
        context,
        thirdUser.Id,
        egypt2024.Id,
        5,
        "Відпочинок у Єгипті залишив дуже позитивне враження. Море, сервіс і загальна організація були на високому рівні.",
        new DateTime(2024, 9, 14, 12, 40, 0));

    await AddReviewIfNotExistsAsync(
        context,
        thirdUser.Id,
        crete2025.Id,
        3,
        "Крит сподобався, але екскурсійна програма була занадто інтенсивною. Для спокійного відпочинку хотілося б більше вільного часу.",
        new DateTime(2025, 7, 21, 16, 10, 0));

    await AddReviewIfNotExistsAsync(
        context,
        thirdUser.Id,
        spain2025.Id,
        5,
        "Коста-Брава стала одним із найкращих напрямів. Тур добре поєднав пляжний відпочинок і культурні маршрути.",
        new DateTime(2025, 8, 27, 18, 30, 0));

    await AddReviewIfNotExistsAsync(
        context,
        thirdUser.Id,
        santorini2026.Id,
        4,
        "Санторіні дуже гарний напрям, проте вартість туру досить висока. Загалом враження позитивні.",
        new DateTime(2026, 3, 24, 19, 15, 0));

    await AddReviewIfNotExistsAsync(
        context,
        thirdUser.Id,
        barcelona2026.Id,
        5,
        "Барселона сподобалася найбільше завдяки поєднанню архітектури, моря та екскурсійної програми.",
        new DateTime(2026, 4, 17, 20, 20, 0));

    await AddFavoriteIfNotExistsAsync(context, firstUser.Id, santorini2026.Id, new DateTime(2026, 2, 18, 12, 0, 0));
    await AddFavoriteIfNotExistsAsync(context, firstUser.Id, crete2025.Id, new DateTime(2025, 6, 10, 12, 0, 0));

    await AddFavoriteIfNotExistsAsync(context, secondUser.Id, barcelona2026.Id, new DateTime(2026, 3, 10, 12, 0, 0));
    await AddFavoriteIfNotExistsAsync(context, secondUser.Id, spain2025.Id, new DateTime(2025, 7, 1, 12, 0, 0));

    await context.SaveChangesAsync();
  }


  private static async Task<ApplicationUser> CreateTestUserIfNotExistsAsync(
    UserManager<ApplicationUser> userManager,
    string email,
    string password,
    string fullName) {
    var user = await userManager.FindByEmailAsync(email);

    if (user != null) {
      if (!await userManager.IsInRoleAsync(user, RoleNames.User)) {
        await userManager.AddToRoleAsync(user, RoleNames.User);
      }

      return user;
    }

    user = new ApplicationUser {
      UserName = email,
      Email = email,
      EmailConfirmed = true,
      FullName = fullName,
      RegisteredAt = DateTime.Now.AddMonths(-8),
      IsBlocked = false
    };

    var result = await userManager.CreateAsync(user, password);

    if (result.Succeeded) {
      await userManager.AddToRoleAsync(user, RoleNames.User);
    }

    return user;
  }

  private static async Task AddBookingIfNotExistsAsync(
      ApplicationDbContext context,
      string userId,
      int tourId,
      DateTime bookingDate,
      string status) {
    bool exists = await context.Bookings
        .AnyAsync(b => b.UserId == userId && b.TourId == tourId);

    if (exists) {
      return;
    }

    var booking = new Booking {
      UserId = userId,
      TourId = tourId,
      BookingDate = bookingDate,
      Status = status
    };

    context.Bookings.Add(booking);
    await context.SaveChangesAsync();
  }

  private static async Task AddReviewIfNotExistsAsync(
      ApplicationDbContext context,
      string userId,
      int tourId,
      int rating,
      string comment,
      DateTime createdAt) {
    bool exists = await context.Reviews
        .AnyAsync(r => r.UserId == userId && r.TourId == tourId);

    if (exists) {
      return;
    }

    var review = new Review {
      UserId = userId,
      TourId = tourId,
      Rating = rating,
      Comment = comment,
      CreatedAt = createdAt
    };

    context.Reviews.Add(review);
    await context.SaveChangesAsync();
  }

  private static async Task AddFavoriteIfNotExistsAsync(
      ApplicationDbContext context,
      string userId,
      int tourId,
      DateTime addedAt) {
    bool exists = await context.FavoriteTours
        .AnyAsync(f => f.UserId == userId && f.TourId == tourId);

    if (exists) {
      return;
    }

    var favorite = new FavoriteTour {
      UserId = userId,
      TourId = tourId,
      AddedAt = addedAt
    };

    context.FavoriteTours.Add(favorite);
    await context.SaveChangesAsync();
  }

}