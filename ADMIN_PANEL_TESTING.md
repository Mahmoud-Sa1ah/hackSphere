# Admin Panel Testing Guide

## What I Fixed

### 1. Frontend Files Modified:
- `frontend-angular/src/app/features/admin/admin.component.ts`
  - Added `notificationTitle` field
  - Added `closeKnowledgeDialog()` method
  - Updated `handleSendNotification()` to pass title parameter

- `frontend-angular/src/app/features/admin/admin.component.html`
  - Added Title input field to notification dialog
  - Added Tags input field to knowledge dialog
  - Added "Publish immediately" checkbox to knowledge dialog
  - Fixed button labels ("Create Article" vs "Update Article")

- `frontend-angular/src/app/features/admin/services/admin.service.ts`
  - Updated `sendNotification()` to accept title parameter

- `frontend-angular/src/app/features/knowledge-base/knowledge.service.ts`
  - Fixed API URL to use `environment.apiUrl`

- `frontend-angular/src/app/shared/components/header/header.component.html`
  - Fixed build error (malformed HTML tag)
  - Limited notification dropdown to ~4 messages with scrolling

## How to Test

### Step 1: Verify Backend is Running
1. Open a terminal in `backend-api` folder
2. Run: `dotnet run` or `dotnet watch`
3. Verify it starts on `https://localhost:53812`
4. Check console for any errors

### Step 2: Start Frontend
1. Open a terminal in `frontend-angular` folder
2. Run: `npm start`
3. Wait for "Compiled successfully"
4. Open browser to `http://localhost:4200`

### Step 3: Login as Admin
1. Login with an admin account
2. Navigate to `/admin` page
3. You should see:
   - Users Management table
   - Knowledge Base Management table
   - Tools Management section
   - Domain Requests section

### Step 4: Test Send Notification
1. Find a user in the Users table
2. Click the **Send** icon (blue envelope icon)
3. **Expected**: A modal dialog should appear with:
   - "Send Notification" title
   - Recipient name shown
   - **Title** input field (optional)
   - **Message** textarea (required)
   - Cancel and "Send Now" buttons
4. Enter a message
5. Click "Send Now"
6. **Expected**: 
   - Success toast: "Notification sent successfully"
   - Dialog closes
   - Check browser Network tab (F12) for POST to `/api/admin/notifications`

### Step 5: Test Add Article
1. Click the **"Add Article"** button (blue, top right of Knowledge Base section)
2. **Expected**: A modal dialog should appear with:
   - "Create Knowledge Article" title
   - **Title** input (required)
   - **Category** dropdown
   - **Content** textarea (required)
   - **Tags** input (comma-separated)
   - **"Publish immediately"** checkbox
   - "Create Article" and Cancel buttons
3. Fill in Title and Content
4. Click "Create Article"
5. **Expected**:
   - Success toast: "Knowledge article created"
   - Dialog closes
   - Article appears in the table
   - Check browser Network tab for POST to `/api/admin/knowledge`

### Step 6: Test Edit Article
1. Find an article in the Knowledge Base table
2. Click the **Edit** icon (yellow pencil)
3. **Expected**: Modal opens with existing article data pre-filled
4. Modify the content
5. Click "Update Article"
6. **Expected**:
   - Success toast: "Knowledge article updated"
   - Dialog closes
   - Check Network tab for PUT to `/api/admin/knowledge/{id}`

## Troubleshooting

### If Nothing Happens When Clicking Buttons:

1. **Open Browser Console** (F12 → Console tab)
   - Look for JavaScript errors
   - Common errors:
     - `Cannot read property 'userId' of null` → User not loaded
     - `HttpErrorResponse` → Backend not running or CORS issue
     - Template errors → Check if page rendered correctly

2. **Check Network Tab** (F12 → Network tab)
   - When you click "Send Now", do you see a request to `/api/admin/notifications`?
   - If NO request appears → Frontend click handler not working
   - If request appears with error → Backend issue

3. **Verify You're Logged In as Admin**
   - Open Console and type: `localStorage.getItem('user')`
   - Check if `role` is `"Admin"`
   - If not admin, you'll be redirected away from `/admin`

4. **Check if Data Loaded**
   - In Console, type: `document.querySelector('table')`
   - Should show the users table
   - If you see "Loading..." forever → API calls failing

### Common Issues:

**Issue**: "Loading..." never goes away
- **Cause**: Backend not running or API calls failing
- **Fix**: Start backend, check proxy.conf.json points to correct port

**Issue**: Buttons don't respond to clicks
- **Cause**: JavaScript error preventing component from working
- **Fix**: Check browser console for errors

**Issue**: Dialog opens but "Send Now" does nothing
- **Cause**: Form validation or HTTP request failing
- **Fix**: Check Network tab for failed requests

**Issue**: Request sent but gets 401 Unauthorized
- **Cause**: JWT token missing or expired
- **Fix**: Re-login, check `localStorage.getItem('token')`

**Issue**: Request sent but gets 403 Forbidden
- **Cause**: User is not Admin role
- **Fix**: Verify user role in database

## What to Report Back

Please tell me:
1. **What happens when you click "Send" icon?**
   - Does dialog open? YES/NO
   - If yes, can you type in the fields? YES/NO
   - When you click "Send Now", what happens?

2. **What happens when you click "Add Article"?**
   - Does dialog open? YES/NO
   - If yes, can you fill in the form? YES/NO
   - When you click "Create Article", what happens?

3. **Browser Console Errors?**
   - Open F12 → Console
   - Copy/paste any RED error messages

4. **Network Tab**
   - Open F12 → Network tab
   - Click "Send Now" on a notification
   - Do you see a request to `/api/admin/notifications`?
   - What is the status code? (200, 401, 403, 500, etc.)
   - Click on the request → Preview tab → What does the response say?

This information will help me identify the exact problem.
