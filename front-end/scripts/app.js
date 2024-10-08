const API_BASE_URL = 'http://localhost:3000/recipes';

const viewTypes = {
    ALL: "all",
    FAVORITES: "favorites",
    SORTED: "sorted",
    SEARCH: "search"
};

let state = {
    currentPage: 1,
    totalPages: 1,
    limit: 8,
    viewType: viewTypes.ALL,
    searchTerm: null
};

async function fetchRecipes() {
    let url = API_BASE_URL;
    let params = {
        page: state.currentPage,
        limit: state.limit
    };

    switch (state.viewType) {
        case viewTypes.FAVORITES:
            url += '/favorites';
            break;
        case viewTypes.SORTED:
            url += '/sort';
            break;
        case viewTypes.SEARCH:
            url += '/search';
            if (state.searchTerm) {
                params.searchTerm = state.searchTerm;
            }
            break;
    }

    try {
        const response = await axios.get(url, { params });
        if (response.status !== 200) {
            throw new Error("Error fetching recipes");
        }
        return response.data;
    } catch (error) {
        showAlert(`An error occurred while fetching ${state.viewType} recipes. Please try again.`);
        return null;
    }
}

async function addRecipe(name, description, category) {
    try {
        await axios.post(API_BASE_URL, {
            name,
            description,
            category,
            favorite: false,
        });

        document.getElementById("rcpName").value = "";
        document.getElementById("rcpDescription").value = "";
        document.getElementById("rcpCategory").value = "Breakfast";

        const paginationInfo = await fetchPaginationInfo();
        if (paginationInfo) {
            const newPage = paginationInfo.totalPages;
            await displayRecipes({ viewType: viewTypes.ALL, currentPage: newPage });
        } else {
            await displayRecipes({ viewType: viewTypes.ALL, currentPage: state.currentPage });
        }

        showAlert("Recipe added successfully!", "success");
    } catch (error) {
        showAlert("An error occurred while adding the recipe. Please try again.");
    }
}

function renderRecipes(recipes) {
    const recipeList = document.getElementById("recipeList");
    recipeList.innerHTML = "";

    recipes.forEach(recipe => {
        const recipeItem = document.createElement("li");
        recipeItem.innerHTML = `
            <span>${recipe.name} - ${recipe.description} - ${recipe.category}</span>
            <div class="actions">
                <button class="favorite-button ${recipe.favorite ? 'favorited' : ''}" title="${recipe.favorite ? 'Unfavorite' : 'Favorite'}">
                    <span class="material-icons">${recipe.favorite ? 'favorite' : 'favorite_border'}</span>
                </button>
                <button class="delete-button" title="Delete">
                    <span class="material-icons">delete</span>
                </button>
                <button class="edit-button" title="Edit">
                    <span class="material-icons">edit</span>
                </button>
            </div>
        `;

        const favoriteButton = recipeItem.querySelector('.favorite-button');
        favoriteButton.addEventListener('click', async () => {
            const newStatus = await toggleRecipeFavorite(recipe.recipeId);
            if (newStatus !== null) {
                updateFavoriteButton(favoriteButton, newStatus);
                recipe.favorite = newStatus;
            }
        });

        recipeItem.querySelector('.delete-button').addEventListener('click', () => deleteRecipe(recipe.recipeId));
        recipeItem.querySelector('.edit-button').addEventListener('click', () => promptUpdateDescription(recipe));

        recipeList.appendChild(recipeItem);
    });
}

async function displayRecipes(newState = {}) {
    Object.assign(state, newState);

    const data = await fetchRecipes();
    if (!data) return;

    document.getElementById("name-descriptionErrorMessage").style.display = "none";

    let recipes, totalCount, currentPage, totalPages;

    if (state.viewType === viewTypes.FAVORITES) {
        totalCount = data.totalFavoriteCount;
        recipes = data.favoriteRecipes;
        currentPage = data.page;
        totalPages = data.totalPages;
    } else {
        totalCount = data.totalCount;
        recipes = data.recipes;
        currentPage = data.page || data.currentPage;
        totalPages = data.totalPages;
    }

    recipes = Array.isArray(recipes) ? recipes : [];

    if (recipes.length === 0 && currentPage > 1) {
        return displayRecipes({ ...newState, currentPage: currentPage - 1 });
    }

    state.currentPage = currentPage;
    state.totalPages = totalPages;

    renderRecipes(recipes);
    updatePaginationControls(state.currentPage, state.totalPages, totalCount);
}

async function fetchPaginationInfo(isFavorites = false) {
    try {
        const url = isFavorites ? `${API_BASE_URL}/favorites` : API_BASE_URL;
        const response = await axios.get(url, {
            params: { page: 1, limit: state.limit }
        });
        return {
            totalCount: isFavorites ? response.data.totalFavoriteCount : response.data.totalCount,
            totalPages: response.data.totalPages
        };
    } catch (error) {
        return null;
    }
}

function updateFavoriteButton(button, isFavorited) {
    const icon = button.querySelector('.material-icons');
    if (isFavorited) {
        button.classList.add("favorited");
        icon.textContent = 'favorite';
        button.title = 'Unfavorite';
    } else {
        button.classList.remove("favorited");
        icon.textContent = 'favorite_border';
        button.title = 'Favorite';
    }
}

async function toggleRecipeFavorite(id) {
    try {
        const response = await axios.patch(`${API_BASE_URL}/${id}/favorite`);
        if (response.status === 200) {
            return response.data;
        } else {
            throw new Error("Error toggling favorite status");
        }
    } catch (error) {
        showAlert("An error occurred while favoriting/unfavoriting the recipe. Please try again.");
        return null;
    }
}

async function deleteRecipe(id) {
    try {
        await axios.delete(`${API_BASE_URL}/${id}`);

        const isFavorites = state.viewType === viewTypes.FAVORITES;
        const paginationInfo = await fetchPaginationInfo(isFavorites);

        if (paginationInfo) {
            let newPage = state.currentPage;
            if (newPage > paginationInfo.totalPages) {
                newPage = paginationInfo.totalPages > 0 ? paginationInfo.totalPages : 1;
            }
            await displayRecipes({ currentPage: newPage, viewType: state.viewType });
        } else {
            await displayRecipes({ currentPage: state.currentPage, viewType: state.viewType });
        }

        showAlert("Recipe deleted successfully", "success");
    } catch (error) {
        showAlert("An error occurred while deleting the recipe. Please try again.", "error");
    }
}

async function updateRecipeDescription(id, description) {
    try {
        const formData = new URLSearchParams();
        formData.append("description", description);

        await axios.put(`${API_BASE_URL}/${id}`, formData);
        await displayRecipes({ currentPage: state.currentPage });

        showAlert("Recipe description updated successfully", "success");
    } catch (error) {
        showAlert("An error occurred while updating the recipe description. Please try again.");
    }
}

function promptUpdateDescription(recipe) {
    if (recipe) {
        showModal(recipe.recipeId, recipe.description);
    }
}

async function generateRecipeReport() {
    try {
        const response = await axios.get(`${API_BASE_URL}/stats`);
        if (response.status === 200) {
            const { totalRecipes, favoriteRecipes, nonFavoriteRecipes } = response.data;
            const reportDiv = document.getElementById("recipeReport");
            reportDiv.innerHTML = `
                <p>Total recipes: ${totalRecipes}</p>
                <p>Number of favorite recipes: ${favoriteRecipes}</p>
                <p>Number of recipes not marked as favorites: ${nonFavoriteRecipes}</p>
            `;
        } else {
            throw new Error("Error fetching data");
        }
    } catch (error) {
        showAlert("An error occurred while creating a report. Please try again.");
    }
}

const modal = document.getElementById("updateDescriptionModal");
const closeButton = document.getElementById("close");
const updateButton = document.getElementById("updateButton");
let currentRecipeId = null;

function showModal(id, currentDescription) {
    currentRecipeId = id;
    const modal = document.getElementById("updateDescriptionModal");
    const newDescriptionField = document.getElementById("newDescription");
    newDescriptionField.value = currentDescription;
    modal.style.display = "flex";
    document.body.style.overflow = "hidden";
}

function closeModal() {
    const modal = document.getElementById("updateDescriptionModal");
    modal.style.display = "none";
    document.getElementById("newDescription").value = "";
    document.getElementById("errorMessage").style.display = "none";
    document.body.style.overflow = "";
}

closeButton.onclick = closeModal;

window.addEventListener("click", (event) => {
    if (event.target === document.getElementById("updateDescriptionModal")) {
        closeModal();
    }
});

document.getElementById("updateButton").addEventListener("click", async () => {
    const newDescription = document.getElementById("newDescription").value;
    const errorMessage = document.getElementById("errorMessage");
    if (newDescription) {
        await updateRecipeDescription(currentRecipeId, newDescription);
        closeModal();
    } else {
        errorMessage.style.display = "block";
    }
});

function showAlert(message, type = "error") {
    const alertDiv = document.getElementById("alertDiv");
    alertDiv.textContent = message;
    alertDiv.className = `alert ${type}`;
    alertDiv.style.display = "block";

    setTimeout(() => {
        alertDiv.style.display = "none";
    }, 6000);
}

function changePage(direction) {
    const newPage = state.currentPage + direction;
    if (newPage >= 1 && newPage <= state.totalPages) {
        displayRecipes({ currentPage: newPage });
    }
}

function updatePaginationControls(currentPage, totalPages, totalCount) {
    const previousButton = document.getElementById("previousPage");
    const nextButton = document.getElementById("nextPage");
    const pageNumber = document.getElementById("pageNumber");

    previousButton.disabled = currentPage <= 1;
    nextButton.disabled = currentPage >= totalPages;

    let viewTypeText = '';
    switch (state.viewType) {
        case viewTypes.FAVORITES:
            viewTypeText = 'Favorite';
            break;
        case viewTypes.SORTED:
            viewTypeText = 'Sorted';
            break;
        case viewTypes.SEARCH:
            viewTypeText = 'Matching';
            break;
        default:
            viewTypeText = '';
    }

    pageNumber.textContent = `Page ${currentPage} of ${totalPages} (Total ${viewTypeText} Recipes: ${totalCount})`;
}

document.getElementById("addRecipeButton").addEventListener("click", () => {
    const recipeName = document.getElementById("rcpName").value.trim();
    const recipeDescription = document.getElementById("rcpDescription").value.trim();
    const recipeCategory = document.getElementById("rcpCategory").value;
    const errorMessage = document.getElementById("name-descriptionErrorMessage");

    if (!recipeName || !recipeDescription) {
        errorMessage.style.display = "block";
    } else {
        errorMessage.style.display = "none";
        addRecipe(recipeName, recipeDescription, recipeCategory);
    }
});

document.getElementById("showAll").addEventListener("click", () => displayRecipes({ viewType: viewTypes.ALL, currentPage: 1 }));
document.getElementById("showFavorites").addEventListener("click", () => displayRecipes({ viewType: viewTypes.FAVORITES, currentPage: 1 }));
document.getElementById("recipeReporter").addEventListener("click", async () => {
    const reportDiv = document.getElementById("recipeReport");
    if (reportDiv.style.display === "none" || !reportDiv.innerHTML) {
        await generateRecipeReport();
        reportDiv.style.display = "block";
    } else {
        reportDiv.style.display = "none";
    }
});
document.getElementById("sortByCategory").addEventListener("click", () => displayRecipes({ viewType: viewTypes.SORTED, currentPage: 1 }));
document.getElementById("searchInput").addEventListener("input", (e) => {
    const searchTerm = e.target.value.trim();
    if (searchTerm) {
        displayRecipes({ viewType: viewTypes.SEARCH, searchTerm, currentPage: 1 });
    } else {
        displayRecipes({ viewType: viewTypes.ALL, currentPage: 1 });
    }
});

document.getElementById("previousPage").addEventListener("click", () => changePage(-1));
document.getElementById("nextPage").addEventListener("click", () => changePage(1));

displayRecipes();