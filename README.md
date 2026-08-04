# HoneyComb0! (Tessellation)

Unity로 개발하는 모바일 퍼즐 게임 프로젝트입니다. Android 빌드와 Google Play 내부 테스트 배포를 GitHub Actions로 관리합니다.

## 개발 환경

| 항목 | 값 |
| --- | --- |
| Unity | `6000.2.7f2` |
| 주요 플랫폼 | Android ARM64 |
| Android applicationId | `com.tessellation.honeycomb0` |
| 최초 CI/CD 릴리스 | `v1.1.1` |

저장소를 clone한 뒤 Unity Hub에서 루트 폴더를 열어 작업합니다. `Library`, `Temp`, `Logs`, `Build` 등 Unity 생성 파일은 커밋하지 않습니다.

## 브랜치 전략

`main`만 장기 유지합니다. 모든 변경은 짧게 유지되는 작업 브랜치에서 개발한 뒤 Pull Request로 병합합니다.

브랜치 이름은 다음 형식을 사용합니다.

```text
<type>/<선택적-이슈번호>-<kebab-case>
```

예시:

```text
feat/33-tile-editor
fix/restart-failure-ui
ci/android-release
```

허용되는 type은 `feat`, `fix`, `refactor`, `perf`, `docs`, `test`, `build`, `ci`, `chore`, `hotfix`입니다. 새 단어는 소문자 영문과 숫자를 하이픈으로 연결합니다. Dependabot 브랜치는 이 규칙에서 제외됩니다.

PR 제목은 다음 Conventional Commit 형식을 사용합니다.

```text
<type>(<선택적 scope>): <설명>
```

예: `feat(tile): 폭탄 타일 생성 규칙 추가`

개별 작업 커밋은 자유롭게 작성할 수 있지만 최종 이력은 PR 제목을 사용하는 Squash Merge로 정리합니다.

### main 보호 규칙

GitHub Ruleset에서 다음 값을 적용합니다.

- 직접 push, force push, 브랜치 삭제 금지
- PR 승인 1명 필수, 새 커밋이 올라오면 기존 승인 무효화
- 모든 리뷰 대화 해결 필수
- 필수 검사: `Policy`, `Unity Tests (EditMode)`, `Unity Tests (PlayMode)`
- Squash Merge만 허용하고 병합된 브랜치는 자동 삭제
- 관리자도 일반 변경에서는 규칙을 우회하지 않음

`v*.*.*` 릴리스 태그는 관리자만 생성하며 수정하거나 삭제하지 않습니다. 긴급 수정도 `hotfix/...` 브랜치와 PR을 거칩니다.

## CI

| 시점 | 검사 |
| --- | --- |
| `main` 대상 PR | 브랜치명, PR 제목, Unity EditMode/PlayMode 테스트, 전체 스크립트 컴파일 |
| CI 작업 브랜치 push | 위 테스트 후 Release 구성의 테스트 APK 생성 |
| `main` 병합 | 위 테스트 후 등록된 release keystore로 서명한 Android APK 생성 |
| `vMAJOR.MINOR.PATCH` 태그 | 서명된 Android App Bundle 생성 및 Draft GitHub Release 작성 |
| 승인된 수동 실행 | 기존 Draft AAB를 Google Play 내부 테스트에 게시 |

테스트 결과는 14일, Release APK는 14일, 릴리스 AAB는 90일 동안 Actions artifact로 보관합니다. 같은 PR의 이전 실행은 취소하지만 릴리스와 Play 배포는 중간 취소하지 않습니다.

CI 작업 브랜치의 테스트 APK는 `Development Build`가 아닌 Release 구성으로 빌드합니다. release keystore Secrets가 모두 등록되어 있으면 해당 키로 서명하고, 모두 없으면 테스트 배포를 위해 Unity 기본 debug keystore로 서명합니다. `main`과 태그 릴리스에서는 release keystore Secrets가 반드시 필요합니다.

CI에서 Unity를 실행하려면 Personal 라이선스의 `UNITY_LICENSE`, 또는 Pro 라이선스의 `UNITY_EMAIL`·`UNITY_PASSWORD`·`UNITY_SERIAL` 조합이 필요합니다. 유효한 라이선스가 없으면 Unity 테스트와 빌드는 실패하도록 두어 보호 규칙을 우회하지 않습니다.

## Android 릴리스

### 필요한 GitHub 설정

Repository secrets:

- `UNITY_LICENSE`
- `UNITY_EMAIL` (Pro 라이선스 방식에서 사용)
- `UNITY_PASSWORD` (Pro 라이선스 방식에서 사용)
- `UNITY_SERIAL` (Pro 라이선스 방식에서 사용)
- `ANDROID_KEYSTORE_BASE64`
- `ANDROID_KEYSTORE_PASS`
- `ANDROID_KEYALIAS_PASS`

키 alias는 저장소의 기존 release key에 맞춰 기본값 `tessellation`을 사용합니다. 다른 alias를 사용한다면 `ANDROID_KEYALIAS_NAME`을 등록합니다. 기존 GameCI 이름인 `KEYSTORE_BASE64`, `KEYSTORE_PASS`, `KEY_ALIAS_NAME`, `KEY_ALIAS_PASS`도 호환됩니다.

`google-play-internal` Environment variables:

- `GCP_WORKLOAD_IDENTITY_PROVIDER`
- `GCP_SERVICE_ACCOUNT`

Environment에는 배포 가능한 관리자만 접근할 수 있게 제한합니다. Google 서비스 계정에는 해당 앱의 내부 테스트 릴리스에 필요한 최소 권한만 부여합니다. JSON 서비스 계정 키 대신 GitHub OIDC와 Google Workload Identity Federation을 사용합니다.

### 최초 Play Console 등록

1. 새 업로드 키를 오프라인에서 생성하고 팀 비밀 저장소에 백업합니다.
2. 위 keystore 관련 Secrets를 등록합니다. keystore 파일이나 비밀번호는 저장소에 커밋하지 않습니다.
3. Play Console에 applicationId `com.tessellation.honeycomb0`인 앱을 만들고 Play App Signing을 활성화합니다.
4. `main`의 검사가 모두 통과한 커밋에 최초 태그를 생성합니다.

   ```powershell
   git tag -a v1.1.1 -m "Android internal v1.1.1"
   git push origin v1.1.1
   ```

5. `Android Release Build`가 만든 Draft GitHub Release에서 AAB를 받아 Play Console 내부 테스트 트랙에 최초 한 번 수동 업로드합니다. Google Play API는 콘솔에 패키지가 존재하기 전에는 자동 업로드할 수 없습니다.
6. Android Publisher API, 서비스 계정, Workload Identity Federation을 연결하고 GitHub Environment variables를 등록합니다.
7. 최초 수동 릴리스의 Draft 상태를 해제합니다. 이후 버전부터 아래 자동 배포 절차를 사용합니다.

### 이후 릴리스

1. 릴리스할 `main` 커밋에 `vMAJOR.MINOR.PATCH` 태그를 push합니다.
2. `Android Release Build`가 성공하고 Draft Release에 AAB가 첨부됐는지 확인합니다.
3. GitHub Actions의 `Publish to Google Play Internal`을 열고 `Run workflow`에서 동일한 태그를 입력합니다.
4. `google-play-internal` 승인을 거치면 Draft에 있던 동일 AAB가 내부 테스트 트랙에 `completed` 상태로 게시됩니다.
5. Play 업로드가 성공한 경우에만 GitHub Release가 공개 상태로 전환됩니다.

태그 `vMAJOR.MINOR.PATCH`는 Unity `bundleVersion`의 `MAJOR.MINOR.PATCH`가 됩니다. Android `versionCode`는 `MAJOR × 1,000,000 + MINOR × 1,000 + PATCH`로 계산됩니다. 배포가 실패하면 Draft를 유지한 채 같은 태그로 Publish workflow를 재시도합니다. 이미 배포한 바이너리는 덮어쓰지 말고 수정 후 patch 버전을 올려 새 태그를 만듭니다.

## 서명 키 보안

과거에 추적된 `KeyStore/Tessellation.keystore`는 폐기 대상입니다. 새 업로드 키를 Secrets에 등록한 뒤 현재 브랜치에서 파일을 제거하고, 팀 작업을 잠시 중단한 상태에서 별도의 mirror clone으로 전체 기록을 정리합니다.

```powershell
git filter-repo --sensitive-data-removal --invert-paths --path KeyStore/Tessellation.keystore
```

기록 정리는 모든 브랜치와 태그의 commit SHA를 바꾸므로 저장소 관리자 한 명이 공지 후 force push해야 합니다. 완료 후 기존 clone은 사용하지 말고 전원이 새로 clone합니다. 키 값, 비밀번호, base64 문자열은 이슈·PR·Actions 로그에 남기지 않습니다.

## 테스트

- EditMode: 활성화된 빌드 씬의 존재, 첫 로딩 씬, Android applicationId, ARM64 설정 검증
- PlayMode: `InitialLoadingScene` 로드 smoke test
- 실제 릴리스 전: `main`에서 생성된 Release APK를 Android 기기에 설치해 시작 화면 진입 확인

로컬 Unity Test Runner에서도 EditMode와 PlayMode를 모두 통과시킨 뒤 PR을 올리는 것을 권장합니다.
