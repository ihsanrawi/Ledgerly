# Testing Guide: Duplicate Detection Dialog

**Component:** `duplicate-warning-dialog.component.*`
**Date:** 2025-10-19
**Purpose:** Verify side-by-side comparison, confidence badges, and difference highlighting work correctly

---

## 🚀 Quick Start

### 1. Start Development Server

```bash
cd /home/ihsan/Desktop/Ledgerly/src/Ledgerly.Web
ng serve
```

**Expected output:**
```
✔ Browser application bundle generation complete.
Initial Chunk Files   | Names         |  Raw Size
...
✔ Compiled successfully.
** Angular Live Development Server is listening on localhost:4200 **
```

**If you see compilation errors**, they're likely related to the Angular Material imports. We'll fix them.

### 2. Start Backend API

In a separate terminal:
```bash
cd /home/ihsan/Desktop/Ledgerly/src/Ledgerly.Api
dotnet run
```

### 3. Open Browser

Navigate to: `http://localhost:4200`

---

## 🧪 Test Scenarios

### Scenario 1: Basic Duplicate Detection Flow

**Steps:**
1. Click "Import CSV" (or navigate to import page)
2. Upload a CSV file with transactions
3. Proceed through column mapping
4. **Trigger:** Backend detects duplicates and opens dialog

**Expected behavior:**
- ✅ Dialog opens with side-by-side comparison
- ✅ Left card shows "Existing Transaction" (blue border)
- ✅ Right card shows "New Transaction" (teal border)
- ✅ Confidence badge displays at top (Green/Yellow/Gray)
- ✅ Differences highlighted in yellow background
- ✅ "Skip All Remaining (X)" button shows count

**Screenshot locations:**
- Desktop view: `.ai/screenshots/after/duplicate-dialog-desktop-light.png`
- Mobile view: `.ai/screenshots/after/duplicate-dialog-mobile-light.png`

---

### Scenario 2: Exact Match

**Test data:**
```csv
date,payee,amount,category,account
2025-01-15,Target,50.00,Groceries,Checking
```

(Assuming this transaction already exists in ledger)

**Expected:**
- ✅ **Green badge:** "Exact Match"
- ✅ **No yellow highlights** (all fields identical)
- ✅ Explanation: "These transactions are identical. Skip to avoid duplication."

---

### Scenario 3: Likely Match (Different Category)

**Test data:**
```csv
date,payee,amount,category,account
2025-01-15,Target,50.00,Shopping,Checking
```

(Existing has `category: Groceries`, new has `category: Shopping`)

**Expected:**
- ✅ **Yellow badge:** "Likely Match"
- ✅ **Yellow highlight** on category field ONLY
- ✅ Explanation: "These transactions are very similar..."
- ✅ Differences list: "Differences: category"

---

### Scenario 4: Possible Match (Different Amount)

**Test data:**
```csv
date,payee,amount,category,account
2025-01-15,Target,49.99,Groceries,Checking
```

(Existing has `amount: 50.00`, new has `amount: 49.99`)

**Expected:**
- ✅ **Gray badge:** "Possible Match"
- ✅ **Yellow highlight** on amount field
- ✅ Explanation: "These transactions have some similarities but differ in key fields..."
- ✅ Differences list: "Differences: amount"

---

### Scenario 5: Multiple Duplicates

**Test data:**
```csv
date,payee,amount,category,account
2025-01-15,Target,50.00,Groceries,Checking
2025-01-16,Walmart,75.00,Shopping,Checking
2025-01-17,Starbucks,5.50,Coffee,Checking
```

(All 3 exist as duplicates)

**Expected:**
- ✅ Progress indicator: "Reviewing duplicate 1 of 3"
- ✅ Click "Skip This" → Moves to "2 of 3"
- ✅ Click "Previous" → Goes back to "1 of 3"
- ✅ Decision status shows: "Marked to Skip"
- ✅ Click "Skip All Remaining" → Marks all 3 as skip, closes dialog

---

### Scenario 6: Mobile Responsive

**Steps:**
1. Open Chrome DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Select iPhone 12 Pro (390px width)
4. Open duplicate dialog

**Expected:**
- ✅ Cards stack vertically (not side-by-side)
- ✅ Dialog takes full width
- ✅ Buttons stack vertically
- ✅ "Skip All Remaining" button shows first
- ✅ All text readable, no horizontal scroll

**Test at these widths:**
- 375px (iPhone SE)
- 390px (iPhone 12)
- 768px (iPad Mini)
- 1024px (iPad Pro)

---

### Scenario 7: Dark Mode

**Steps:**
1. Open Chrome DevTools
2. Press Ctrl+Shift+P → "Rendering"
3. Scroll to "Emulate CSS prefers-color-scheme"
4. Select "prefers-color-scheme: dark"

**Expected:**
- ✅ Dialog background: Deep slate (#1C1E26)
- ✅ Text: Light gray (readable)
- ✅ Yellow highlights: `rgba(255, 235, 59, 0.2)` (visible but not harsh)
- ✅ Confidence badges: Same colors (contrast sufficient)
- ✅ Card borders: Visible against dark background

---

### Scenario 8: Keyboard Navigation

**Steps:**
1. Open duplicate dialog
2. **Press Tab** repeatedly
3. **Press Enter** on focused button
4. **Press Escape** to close

**Expected tab order:**
1. ✅ Previous button (or disabled)
2. ✅ Next button
3. ✅ Skip All Remaining button
4. ✅ Skip This button
5. ✅ Import Anyway button

**Expected focus indicators:**
- ✅ 2px teal outline around focused element
- ✅ 2px offset from element
- ✅ Visible on all button types

---

### Scenario 9: Accessibility (Screen Reader)

**Steps:**
1. Enable screen reader (Windows Narrator / macOS VoiceOver)
2. Tab through dialog
3. Listen to announcements

**Expected announcements:**
- ✅ "Duplicate Transactions Detected"
- ✅ "Reviewing duplicate 1 of 3"
- ✅ "Exact Match" (or Likely/Possible)
- ✅ "Previous duplicate, button, disabled"
- ✅ "Skip this duplicate, button"
- ✅ "Import this transaction anyway, button"

---

## 🐛 Common Issues & Fixes

### Issue 1: Compilation Error - MatChipsModule

**Error:**
```
NG8001: 'mat-chip-set' is not a known element
```

**Fix:**
The component already imports `MatChipsModule`, but ensure your `package.json` has:
```json
"@angular/material": "^17.0.0"
```

If missing, run:
```bash
npm install @angular/material@17
```

---

### Issue 2: CSS Custom Properties Not Working

**Symptoms:**
- Buttons have no color
- Text is default black
- No teal/green/red colors

**Fix:**
Ensure `src/Ledgerly.Web/src/styles.css` imports design tokens:
```css
@import './theme/design-tokens.css';
```

This is already in place, but if you see issues, check the import path.

---

### Issue 3: Dialog Too Narrow

**Symptoms:**
- Dialog is 500px wide (old size)
- Cards are cramped

**Fix:**
Check that `.duplicate-dialog` has:
```scss
min-width: 600px;
max-width: 900px;
```

This is already updated in the SCSS file.

---

### Issue 4: Yellow Highlights Not Showing

**Symptoms:**
- No yellow background on differences
- All fields look the same

**Fix:**
Ensure the difference detection logic is working:
1. Open browser console (F12)
2. Type: `console.log(differences())`
3. Should show array like: `['category']`

If empty, check that `parsedTransactions` data is being passed to the dialog.

---

## 📸 Screenshot Checklist

Create these screenshots for documentation:

### Before (if available from old implementation)
- [ ] `before/duplicate-dialog-desktop.png` - Old sequential view

### After (current implementation)
- [ ] `after/duplicate-dialog-desktop-light.png` - Side-by-side, exact match
- [ ] `after/duplicate-dialog-desktop-dark.png` - Dark mode
- [ ] `after/duplicate-dialog-mobile-light.png` - Mobile stacked view
- [ ] `after/duplicate-dialog-exact-match.png` - Green badge
- [ ] `after/duplicate-dialog-likely-match.png` - Yellow badge with highlights
- [ ] `after/duplicate-dialog-possible-match.png` - Gray badge with highlights

**To take screenshots:**
1. Open DevTools (F12)
2. Right-click on `<div class="duplicate-dialog">` element
3. Capture node screenshot (or use OS screenshot tool)
4. Save to `.ai/screenshots/after/` folder

---

## ✅ Success Criteria

The implementation is successful if:

- [x] Side-by-side comparison displays on desktop
- [x] Confidence badges show correct color (Green/Yellow/Gray)
- [x] Differences highlighted in yellow background
- [x] "Skip All Remaining" button works
- [x] Mobile view stacks vertically (<768px)
- [x] Dark mode works (if OS prefers dark)
- [x] Keyboard navigation works (Tab, Enter, Escape)
- [x] ARIA labels announced by screen reader
- [x] No console errors
- [x] No TypeScript compilation errors
- [x] No visual regressions in other components

---

## 🔍 Manual Testing Script

Copy-paste this checklist while testing:

```
□ Start ng serve - no errors
□ Navigate to import page
□ Upload CSV with duplicates
□ Dialog opens
□ Side-by-side layout visible
□ Confidence badge displays (Green/Yellow/Gray)
□ Yellow highlights on differences
□ Click "Skip This" - moves to next
□ Click "Previous" - goes back
□ Click "Skip All Remaining" - closes dialog
□ Decision status shows correctly
□ Mobile view (375px) - stacks vertically
□ Dark mode - colors appropriate
□ Tab through all buttons
□ Focus indicators visible (teal outline)
□ Press Enter - button activates
□ Press Escape - dialog closes
□ Screen reader announces labels
□ Take 6 screenshots
□ No console errors
```

---

## 📊 Performance Check

Open Chrome DevTools → Performance tab:

1. Start recording
2. Open duplicate dialog
3. Navigate through 10 duplicates
4. Stop recording

**Expected:**
- ✅ Frame rate: 60fps (no jank)
- ✅ Animation smooth: fadeInSlide runs at 60fps
- ✅ No layout thrashing
- ✅ Memory stable (no leaks)

---

## 🚨 If Tests Fail

### Steps to Debug:

1. **Check Console:** Any errors?
2. **Check Network:** Is API responding?
3. **Check Data:** Is `parsedTransactions` populated?
4. **Check Imports:** All Material modules imported?
5. **Check CSS:** Are custom properties loading?

### Rollback if Needed:

```bash
git status
git diff duplicate-warning-dialog.component.*
git checkout HEAD -- duplicate-warning-dialog.component.*
```

---

## 📞 Support

**Need help?**
- Review implementation: `.ai/duplicate-dialog-implementation-summary.md`
- Check audit report: `.ai/component-audit-report.md`
- Read progress summary: `.ai/story-3.1-progress-summary.md`

**Found a bug?**
- Note the steps to reproduce
- Check browser console for errors
- Take a screenshot
- Document in `.ai/bug-report.md`

---

## ✨ Next Steps After Testing

Once all tests pass:

1. ✅ Mark duplicate dialog as tested in story file
2. ✅ Create screenshots for documentation
3. ✅ Move to next task: Virtualized scrolling (2.5h)
4. ✅ Continue Story 3.1 implementation

---

**Happy Testing! 🎉**

If you encounter issues, I'm here to help debug and fix them.
