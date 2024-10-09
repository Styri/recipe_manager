# Recipe Manager

This is a a revamped version of my previous web-based recipe manager application with a new back-end(now C#/ASP.NET Core), design and Javascript code. 

## Application Features

- Add new recipes with name, description, and category.
- Update recipe descriptions.
- Delete recipes.
- Toggling favorite status of recipes.
- Search for recipes by name, description, or category.
- Generate a report of total number of recipes and favorites/non-favorites.

## API Features

- **RESTful APIs**: The APIs are designed following RESTful principles, providing clear and structured endpoints.
  
- **Pagination**: Support for pagination in the recipe retrieval endpoints to handle large dataset.

- **Full CRUD Operations**: The API supports Create, Read, Update, and Delete operations.

## Technologies Used

- **Front-end:** HTML, SCSS, JavaScript
- **Back-end:** C#/ASP.NET CORE
- **Database:** SQLite

## Prerequisites

- **.NET SDK 8.0.8**
- **Download it here:** https://dotnet.microsoft.com/en-us/download
- **Any modern browser**

## Running the Application Locally

1. Open any command line/terminal

2. Clone the repository
```bash
    git clone https://github.com/Styri/recipe_manager.git    
```
3. Navigate to the root of the back-end folder.
```bash
    cd recipe_manager
    cd back-end
```

4. Run the application. This will open a new tab in your default browser that serves the index.html found in the root of the front-end folder.
```bash
    dotnet run
```
5. To stop the application, press Ctrl+C in the terminal where the application is running.
   
## Next Steps

- Swagger API documentation
- Authentication

## Contributing

If you would like to contribute, fork the repository and make changes as you'd like.

## License

This project is licensed under the MIT License.
