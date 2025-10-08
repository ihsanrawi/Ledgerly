import { Component, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatTreeModule, MatTreeNestedDataSource } from '@angular/material/tree';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NestedTreeControl } from '@angular/cdk/tree';

interface BalanceDto {
  account: string;
  balance: number;
  depth: number;
  children: BalanceDto[];
}

interface BalanceResponse {
  balances: BalanceDto[];
  asOfDate: string;
}

@Component({
  selector: 'app-balance-display',
  standalone: true,
  imports: [
    CommonModule,
    MatTreeModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './balance-display.component.html',
  styleUrls: ['./balance-display.component.scss']
})
export class BalanceDisplayComponent implements OnInit {
  balances = signal<BalanceDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  treeControl = new NestedTreeControl<BalanceDto>(node => node.children);
  dataSource = new MatTreeNestedDataSource<BalanceDto>();

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.fetchBalances();
  }

  hasChild = (_: number, node: BalanceDto) => !!node.children && node.children.length > 0;

  private fetchBalances(): void {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<BalanceResponse>('http://localhost:5000/api/balance')
      .subscribe({
        next: (response) => {
          this.balances.set(response.balances);
          this.dataSource.data = response.balances;
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to load balances: ' + err.message);
          this.loading.set(false);
        }
      });
  }

  refresh(): void {
    this.fetchBalances();
  }
}
