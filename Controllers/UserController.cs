using Microsoft.AspNetCore.Mvc;
using BiddingSystem.Data;
using BiddingSystem.Models;

namespace BiddingSystem.Controllers
{
    //public class UserController : Controller
    //{
    //    private readonly IUserRepository _userRepository;

    //    public UserController(IUserRepository userRepository)
    //    {
    //        _userRepository = userRepository;
    //    }

    //    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "")
    //    {
    //        var users = await _userRepository.GetAllUsersAsync(page, pageSize, search);
    //        ViewBag.Search = search;
    //        return View(users);
    //    }

    //    public async Task<IActionResult> Details(int id)
    //    {
    //        var user = await _userRepository.GetByIdAsync(id);
    //        if (user == null)
    //            return NotFound();

    //        return View(user);
    //    }

    //    public IActionResult Create()
    //    {
    //        return View();
    //    }

    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Create(User user)
    //    {
    //        if (await _userRepository.EmailExistsAsync(user.Email))
    //        {
    //            ModelState.AddModelError("Email", "This email address is already registered.");
    //        }

    //        if (ModelState.IsValid)
    //        {
    //            var userId = await _userRepository.CreateUserAsync(user);
    //            TempData["SuccessMessage"] = "User created successfully!";
    //            return RedirectToAction(nameof(Index));
    //        }

    //        return View(user);
    //    }

    //    public async Task<IActionResult> Edit(int id)
    //    {
    //        var user = await _userRepository.GetUserByIdAsync(id);
    //        if (user == null)
    //            return NotFound();

    //        return View(user);
    //    }

    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Edit(int id, User user)
    //    {
    //        if (id != user.Id)
    //            return NotFound();

    //        if (await _userRepository.ExistsAsync(user.Email, user.Id))
    //        {
    //            ModelState.AddModelError("Email", "This email address is already registered.");
    //        }

    //        if (ModelState.IsValid)
    //        {
    //            var success = await _userRepository.UpdateAsync(user);
    //            if (success)
    //            {
    //                TempData["SuccessMessage"] = "User updated successfully!";
    //                return RedirectToAction(nameof(Index));
    //            }
    //            else
    //            {
    //                TempData["ErrorMessage"] = "Failed to update user.";
    //            }
    //        }

    //        return View(user);
    //    }

    //    public async Task<IActionResult> Delete(int id)
    //    {
    //        var user = await _userRepository.GetByIdAsync(id);
    //        if (user == null)
    //            return NotFound();

    //        return View(user);
    //    }

    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> DeleteConfirmed(int id)
    //    {
    //        var success = await _userRepository.DeleteAsync(id);
    //        if (success)
    //        {
    //            TempData["SuccessMessage"] = "User deleted successfully!";
    //        }
    //        else
    //        {
    //            TempData["ErrorMessage"] = "Failed to delete user.";
    //        }

    //        return RedirectToAction(nameof(Index));
    //    }
    //}
}
