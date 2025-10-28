# Duplicate Detection Testing - CSV Files

**Purpose:** Test the new side-by-side duplicate detection dialog
**Story:** 3.1 - UI/UX Refinement
**Component:** `duplicate-warning-dialog.component.*`

---

## 📁 Test Files

### 1. `duplicate-test.csv` - Basic Scenarios

**File location:** `/home/ihsan/Desktop/Ledgerly/tests/test-data/duplicate-test.csv`

**Scenarios included:**

| Row | Scenario | Expected Confidence | Expected Differences |
|-----|----------|-------------------|---------------------|
| 1-2 | **Exact Match** | 🟢 Green - "Exact Match" | None (all fields identical) |
| 3-4 | **Likely Match** | 🟡 Yellow - "Likely Match" | Category only (Shopping vs Groceries) |
| 5-6 | **Possible Match** | ⚪ Gray - "Possible Match" | Amount ($5.50 vs $5.49) |
| 7-8 | **Likely Match** | 🟡 Yellow - "Likely Match" | Account only (Credit Card vs Checking) |
| 9 | No duplicate | N/A | Single transaction |
| 10 | No duplicate | N/A | Single transaction |

**Total transactions:** 10
**Expected duplicates:** 4 pairs (8 transactions with duplicates, 2 unique)

---

### 2. `duplicate-scenarios.csv` - Comprehensive Testing

**File location:** `/home/ihsan/Desktop/Ledgerly/tests/test-data/duplicate-scenarios.csv`

**Scenarios included:**

| Rows | Payee | Scenario | Expected Badge | Differences |
|------|-------|----------|---------------|-------------|
| 1-2 | Target | Category difference | 🟡 Yellow | Category: Groceries → Shopping |
| 3-4 | Walmart | Exact match | 🟢 Green | None |
| 5-6 | Starbucks | Amount difference (penny) | ⚪ Gray | Amount: $5.50 → $5.49 |
| 7-8 | Amazon | Exact match | 🟢 Green | None |
| 9-10 | Shell | Amount + Category | ⚪ Gray | Amount: $42.00 → $42.01, Category: Transportation → Fuel |
| 11 | Whole Foods | No duplicate | N/A | Single transaction |
| 12 | CVS | No duplicate | N/A | Single transaction |
| 13 | Netflix | No duplicate | N/A | Single transaction |
| 14 | Trader Joes | No duplicate | N/A | Single transaction |

**Total transactions:** 14
**Expected duplicates:** 5 pairs (10 duplicates)
**Unique transactions:** 4

---

## 🧪 How to Test

### Step 1: First Import (Create Baseline)

**Note:** For the duplicate detection to work, you need transactions already in your ledger. The first import creates the baseline.

1. Open `http://localhost:4200/`
2. Navigate to CSV Import page
3. Upload `duplicate-test.csv`
4. Complete the import process (map columns, handle any issues)
5. **Import all transactions** - this creates your baseline ledger data

### Step 2: Re-Import Same File (Trigger Duplicates)

1. Navigate back to CSV Import page
2. Upload the **same file** (`duplicate-test.csv`) again
3. Complete column mapping
4. **Now you should see the duplicate dialog!** 🎉

### Step 3: Test Each Duplicate

Walk through each duplicate and verify:

**Duplicate 1 of 4 - Target (Exact Match):**
- ✅ Green badge: "Exact Match"
- ✅ No yellow highlights
- ✅ Explanation: "These transactions are identical"
- ✅ Both cards show identical data
- ✅ Progress: "Reviewing duplicate 1 of 4"

**Duplicate 2 of 4 - Walmart (Likely Match):**
- ✅ Yellow badge: "Likely Match"
- ✅ Yellow highlight on "Category" field
- ✅ Left card: "Shopping"
- ✅ Right card: "Groceries"
- ✅ Differences list: "Differences: category"

**Duplicate 3 of 4 - Starbucks (Possible Match):**
- ✅ Gray badge: "Possible Match"
- ✅ Yellow highlight on "Amount" field
- ✅ Left card: "$5.50"
- ✅ Right card: "$5.49"
- ✅ Differences list: "Differences: amount"

**Duplicate 4 of 4 - Amazon (Likely Match):**
- ✅ Yellow badge: "Likely Match"
- ✅ Yellow highlight on "Account" field
- ✅ Left card: "Credit Card"
- ✅ Right card: "Checking"

---

## 🎯 Test Cases

### Test Case 1: Side-by-Side Layout

**Steps:**
1. Open duplicate dialog
2. Verify layout

**Expected (Desktop 1366px):**
- ✅ Two cards side-by-side
- ✅ Left card has blue left border
- ✅ Right card has teal left border
- ✅ Equal width cards
- ✅ Both cards show all 5 fields (Date, Payee, Amount, Category, Account)

**Expected (Mobile 375px):**
- ✅ Cards stack vertically
- ✅ Each card full width
- ✅ Borders still visible

---

### Test Case 2: Confidence Badges

**Exact Match Test:**
- 🟢 Green chip with checkmark icon
- Text: "Exact Match"
- Background: var(--success-color)
- White text

**Likely Match Test:**
- 🟡 Yellow chip with warning icon
- Text: "Likely Match"
- Background: var(--warning-color)
- Black text

**Possible Match Test:**
- ⚪ Gray chip with question icon
- Text: "Possible Match"
- Background: var(--muted-background)
- Gray text with border

---

### Test Case 3: Difference Highlighting

**Steps:**
1. Navigate to a duplicate with differences (e.g., Walmart)
2. Look at the differing field

**Expected:**
- ✅ Yellow background (#fff3cd in light mode)
- ✅ Yellow border (1px solid #ffeb3b)
- ✅ Thicker font weight (semibold)
- ✅ Small padding around value
- ✅ Rounded corners

**Dark mode:**
- ✅ Yellow background: rgba(255, 235, 59, 0.2)
- ✅ Still visible and readable

---

### Test Case 4: Navigation & Actions

**Previous/Next Buttons:**
- ✅ "Previous" disabled on first duplicate
- ✅ "Next" disabled on last duplicate
- ✅ Clicking "Next" moves to next duplicate
- ✅ Clicking "Previous" goes back
- ✅ Progress indicator updates: "1 of 4" → "2 of 4"

**Action Buttons:**
- ✅ "Skip This" - marks as skip, moves to next
- ✅ "Import Anyway" - marks as import, moves to next
- ✅ "Skip All Remaining" - shows count (e.g., "Skip All Remaining (4)")
- ✅ "Skip All Remaining" disabled on last duplicate

**Decision Status:**
- ✅ After clicking "Skip This", shows "Marked to Skip" with red icon
- ✅ After clicking "Import Anyway", shows "Marked to Import" with green icon
- ✅ Can navigate back and see previous decision

---

### Test Case 5: Skip All Remaining

**Steps:**
1. Open duplicate dialog with 4 duplicates
2. Review first duplicate
3. Click "Skip All Remaining (4)"

**Expected:**
- ✅ Dialog closes immediately
- ✅ All 4 duplicates marked as skip
- ✅ Import continues without those 4 transactions
- ✅ No more duplicate dialogs appear

---

### Test Case 6: Keyboard Navigation

**Steps:**
1. Open duplicate dialog
2. Press Tab repeatedly
3. Press Enter on focused button
4. Press Escape

**Expected tab order:**
1. ✅ Previous button (focus indicator visible)
2. ✅ Next button
3. ✅ Skip All Remaining button
4. ✅ Skip This button
5. ✅ Import Anyway button

**Focus indicators:**
- ✅ 2px teal outline (var(--accent-color))
- ✅ 2px offset from element
- ✅ Visible on all buttons

**Escape key:**
- ✅ Closes dialog
- ✅ Returns to import flow

---

### Test Case 7: Responsive Design

**Desktop (1366px):**
```
┌─────────────────────────────────────────────┐
│   Duplicate Transactions Detected           │
│   [Green Badge: Exact Match]                │
│                                             │
│  ┌──────────────┐  ┌──────────────┐       │
│  │  Existing    │  │  New         │       │
│  │  Transaction │  │  Transaction │       │
│  └──────────────┘  └──────────────┘       │
│                                             │
│  [Previous] [Next]                          │
│  [Skip All] [Skip This] [Import Anyway]     │
└─────────────────────────────────────────────┘
```

**Mobile (375px):**
```
┌─────────────────────┐
│  Duplicate          │
│  Transactions       │
│  [Green Badge]      │
│                     │
│  ┌───────────────┐ │
│  │  Existing     │ │
│  │  Transaction  │ │
│  └───────────────┘ │
│                     │
│  ┌───────────────┐ │
│  │  New          │ │
│  │  Transaction  │ │
│  └───────────────┘ │
│                     │
│  [Previous] [Next] │
│  [Skip All]        │
│  [Skip This]       │
│  [Import Anyway]   │
└─────────────────────┘
```

---

## 📸 Screenshots to Capture

Create these screenshots for documentation:

**Desktop Views:**
1. `duplicate-dialog-exact-match-desktop.png` - Green badge, no highlights
2. `duplicate-dialog-likely-match-desktop.png` - Yellow badge, category highlighted
3. `duplicate-dialog-possible-match-desktop.png` - Gray badge, amount highlighted
4. `duplicate-dialog-navigation-desktop.png` - Shows "2 of 4" progress
5. `duplicate-dialog-decision-status-desktop.png` - Shows "Marked to Skip"

**Mobile Views:**
6. `duplicate-dialog-mobile-stacked.png` - Cards stacked vertically
7. `duplicate-dialog-mobile-buttons.png` - Buttons stacked

**Dark Mode:**
8. `duplicate-dialog-dark-mode.png` - All elements in dark theme
9. `duplicate-dialog-dark-highlights.png` - Yellow highlights in dark mode

**Save to:** `.ai/screenshots/after/`

---

## 🐛 Troubleshooting

### Problem: No duplicate dialog appears

**Possible causes:**
1. ❌ No existing transactions in ledger → Import the CSV once first
2. ❌ Backend not running → Start `dotnet run` in Ledgerly.Api
3. ❌ Backend not detecting duplicates → Check API logs

**Solution:**
```bash
# Check backend is running
curl http://localhost:5000/api/balance

# If not running:
cd /home/ihsan/Desktop/Ledgerly/src/Ledgerly.Api
dotnet run
```

---

### Problem: Yellow highlights not showing

**Possible causes:**
1. ❌ All fields identical (exact match) → Expected, should show green badge
2. ❌ CSS custom properties not loading → Check browser console
3. ❌ Difference detection not working → Check component logic

**Debug:**
Open browser console and check:
```javascript
// Should show array of different fields
console.log('Differences:', differences())
```

---

### Problem: Cards not side-by-side

**Possible causes:**
1. ❌ CSS not loading → Check Network tab for 404s
2. ❌ Browser too narrow → Resize to >768px
3. ❌ CSS Grid not supported → Update browser

**Solution:**
- Ensure browser width > 768px
- Hard refresh (Ctrl+Shift+R)
- Check browser console for errors

---

### Problem: Buttons not working

**Possible causes:**
1. ❌ TypeScript error → Check browser console
2. ❌ Event handlers not bound → Check component TypeScript
3. ❌ Dialog data not passed correctly → Check import component

**Debug:**
```javascript
// In browser console
console.log('Current index:', currentIndex())
console.log('Total duplicates:', totalDuplicates())
console.log('Decisions:', decisions())
```

---

## ✅ Success Criteria

All these should be TRUE:

- [x] Build completes without errors (npm run build)
- [x] Dev server starts successfully (ng serve)
- [ ] Duplicate dialog opens when re-importing same CSV
- [ ] Side-by-side layout displays on desktop (>768px)
- [ ] Cards stack vertically on mobile (<768px)
- [ ] Confidence badges show correct color (Green/Yellow/Gray)
- [ ] Yellow highlights appear on different fields
- [ ] "Skip This" marks duplicate and moves to next
- [ ] "Import Anyway" marks duplicate and moves to next
- [ ] "Skip All Remaining" closes dialog and skips all
- [ ] Previous/Next navigation works
- [ ] Keyboard navigation works (Tab, Enter, Escape)
- [ ] Focus indicators visible (teal outline)
- [ ] Dark mode works (yellow highlights visible)
- [ ] No console errors
- [ ] No visual regressions

---

## 🎓 Learning Outcomes

After testing, you should understand:

1. **How duplicate detection works** - Backend compares transactions, frontend displays comparison
2. **Confidence algorithm** - Exact (0 diffs) → Likely (1-2 minor diffs) → Possible (major diffs)
3. **Difference highlighting** - Yellow background automatically applied to differing fields
4. **Responsive design** - Grid layout adapts to screen size
5. **Design system** - CSS custom properties provide consistent theming
6. **Accessibility** - ARIA labels, keyboard navigation, focus indicators

---

## 📞 Need Help?

**CSV not importing?**
- Check backend logs: `cd src/Ledgerly.Api && dotnet run`
- Verify CSV format matches hledger requirements

**Dialog looks wrong?**
- Check browser console for errors
- Verify CSS custom properties loaded
- Try hard refresh (Ctrl+Shift+R)

**Duplicates not detected?**
- Import the CSV once first to create baseline
- Re-import the same CSV to trigger duplicates
- Check backend duplicate detection logic

---

**Happy Testing! 🎉**

If you find any bugs or issues, document them in `.ai/bug-report.md` with:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Screenshots
- Browser console errors
